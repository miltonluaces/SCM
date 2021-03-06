#region Imports

using System;

#endregion

namespace Statistics {

	/** Method:  Metodos del opensource R package */
    	internal class AdditStatFunctions {
        
        #region Fields
		
        private double DBL_EPSILON = 2.220446049250313e-16;
		private double SIXTEN = 16;
		private double M_LN2 = 0.693147180559945309417232121458;
		private double DBL_MIN = 2.2250738585072014e-308;
		private double M_SQRT_32 = 5.656854249492380195206754896838;
		private double M_1_SQRT_2PI = 0.398942280401432677939946059934;
		private double M_2PI = 6.283185307179586476925286766559;
		private double DBL_MAX = 1.7976931348623158e+308;
        private StatFunctions stat;
		
        #endregion 

        #region Constructor 

        internal AdditStatFunctions() {
            stat = new StatFunctions();
		}

        #endregion


        #region internal Methods

        /** Method: 
		 Devuelve el quantil p de una distribución chi2 con df grados de libertad.
		 Si lower_tail = true devuelve la cola de la izquierda, sino la de la derecha.
		 Si log_p = true, devuelve el logaritmo neperiano del quantil, sino el quantil.
		p - Quantil que se busca.
		df - Grados de libertad de la chi2.
		lower_tail - Si lower_tail = true devuelve la cola de la izquierda, sino la de la derecha.
		log_p - Si log_p = true, devuelve el logaritmo neperiano del quantil, sino el quantil. */
        internal double qchisq(double p, double df, bool lower_tail, bool log_p) {
			return qgamma(p, 0.5 * df, 2.0, lower_tail, log_p);
		}


		/** Method: 
		 Devuelve el quantil p de una distribución gamma.
		 Si lower_tail = true devuelve la cola de la izquierda, sino la de la derecha.
		 Si log_p = true, devuelve el logaritmo neperiano del quantil, sino el quantil.
		 Si se desea comparar con lo que devuelve R, entonces el parametro rate de R, 
		 debe ser siempre 0.
		  
		p - Quantil que se busca.
		alpha - Parametro alpha de la distribución gamma.
		scale - Parametro de escala de la distribución gamma.
		lower_tail - Si lower_tail = true devuelve la cola de la izquierda, sino la de la derecha.
		log_p - Si log_p = true, devuelve el logaritmo neperiano del quantil, sino el quantil.*/
        internal double qgamma(double p, double alpha, double scale, bool lower_tail, bool log_p) {
            const double EPS1 = 1e-2, EPS2 = 5e-7, EPS_N = 1e-15, MAXIT = 1000;
            const double pMIN = 1e-100, pMAX = (1 - 1e-14);

            const double i420 = 1.0 / 420.0, i2520 = 1.0 / 2520.0, i5040 = 1.0 / 5040;

            double p_, a, b, c, g, ch, ch0, p1;
            double p2, q, s1, s2, s3, s4, s5, s6, t, x;
            int i, max_it_Newton = 1;

            /* test arguments and initialise */

            if (Double.IsNaN(p) || Double.IsNaN(alpha) || Double.IsNaN(scale))
                return p + alpha + scale;

            if (R_Q_P01_check(p, log_p))
                throw new ArgumentException("Invalid arguments to the quantil gamma function R qgamma Invalid p and or log p");

            if (alpha <= 0) throw new ArgumentException("Invalid arguments to the quantil gamma function R qgamma Invalid scale");

            // Substituir p_ = R_DT_qIv(p); por lo que se muestra a continuación.

            p_ = R_DT_qIv(p, log_p, lower_tail); /* lower_tail prob (in any case) */

            // El código fuente original es g = lgammafn(alpha);/* log Gamma(v/2) */
            // Se substituye por el siguiente que utiliza lo ya escrito
            g = stat.lngamma(alpha);

            /*----- Phase I : Starting Approximation */
            ch = qchisq_appr(p, /* nu= 'df' =  */ 2 * alpha, /* lgamma(nu/2)= */ g,
                lower_tail, log_p, /* tol= */ EPS1);
            if (!R_finite(ch))
            {
                /* forget about all iterations! */
                max_it_Newton = 0; goto END;
            }
            if (ch < EPS2)
            {/* Corrected according to AS 91; MM, May 25, 1999 */
                max_it_Newton = 20;
                goto END;/* and do Newton steps */
            }

            /* FIXME: This (cutoff to {0, +Inf}) is far from optimal
             * -----  when log_p or !lower_tail : */
            if (p_ > pMAX || p_ < pMIN)
            {
                /* did return ML_POSINF or 0.;	much better: */
                max_it_Newton = 20;
                goto END;/* and do Newton steps */
            }

            /*----- Phase II: Iteration
             *	Call pgamma() [AS 239]	and calculate seven term taylor series
             */
            c = alpha - 1;
            s6 = (120.0 + c * (346.0 + 127.0 * c)) * i5040; /* used below, is "const" */

            ch0 = ch;/* save initial approx. */
            for (i = 1; i <= MAXIT; i++)
            {
                q = ch;
                p1 = 0.5 * ch;
                p2 = p_ - pgamma(p1, alpha, 1, /*lower_tail*/ true, /*log_p*/ false);

                if (!R_finite(p2))
                {
                    ch = ch0;
                    max_it_Newton = 27;
                    goto END;
                }/*was  return ML_NAN;*/

                t = p2 * Math.Exp(alpha * M_LN2 + g + p1 - c * Math.Log(ch));
                b = t / ch;
                a = 0.5 * t - b * c;
                s1 = (210.0 + a * (140.0 + a * (105.0 + a * (84.0 + a * (70.0 + 60.0 * a))))) * i420;
                s2 = (420.0 + a * (735.0 + a * (966.0 + a * (1141.0 + 1278.0 * a)))) * i2520;
                s3 = (210.0 + a * (462.0 + a * (707.0 + 932.0 * a))) * i2520;
                s4 = (252.0 + a * (672.0 + 1182.0 * a) + c * (294.0 + a * (889.0 + 1740.0 * a))) * i5040;
                s5 = (84.0 + 2264.0 * a + c * (1175.0 + 606.0 * a)) * i2520;

                ch += t * (1.0 + 0.5 * t * s1 - b * c * (s1 - b * (s2 - b * (s3 - b * (s4 - b * (s5 - b * s6))))));
                if (Math.Abs(q - ch) < EPS2 * ch) goto END;
            }
        /* no convergence in MAXIT iterations -- but we add Newton now... */
        /* was
         *    ML_ERROR(ME_PRECISION);
         * does nothing in R !*/

            END:
            /* PR# 2214 :	 From: Morten Welinder <terra@diku.dk>, Fri, 25 Oct 2002 16:50
               --------	 To: R-bugs@biostat.ku.dk     Subject: qgamma precision

               * With a final Newton step, double accuracy, e.g. for (p= 7e-4; nu= 0.9)
               *
               * Improved (MM): - only if rel.Err > EPS_N (= 1e-15);
               *		    - also for lower_tail = FALSE	 or log_p = TRUE
               * 		    - optionally *iterate* Newton
               */
            x = 0.5 * scale * ch;

            if (max_it_Newton != 0.0)
                p_ = pgamma(x, alpha, scale, lower_tail, log_p);
            for (i = 1; i <= max_it_Newton; i++)
            {
                p1 = p_ - p;
                if (Math.Abs(p1) < Math.Abs(EPS_N * p))
                    break;
                /* else */
                if ((g = dgamma(x, alpha, scale, log_p)) == R_D_0(log_p))
                {
                    break;
                }
                t = log_p ? p1 * Math.Exp(p_ - g) : p1 / g;/* = "delta x" */
                t = lower_tail ? x - t : x + t;
                p_ = pgamma(t, alpha, scale, lower_tail, log_p);
                if (Math.Abs(p_ - p) > Math.Abs(p1) ||
                    (i > 1 && Math.Abs(p_ - p) == Math.Abs(p1)) /* <- against flip-flop */)
                {
                    /* no improvement */
                    break;
                } /* else : */
                x = t;
            }
            return x;
        }

		/** Method: 
		 Compute the Exponential minus 1.
		 accurately also when x is close to zero, i.e. |sample| is near 1.
		x - Value for whcih to calculate Math (x) – 1. */
        internal double expm1(double x) {
			double y, a = Math.Abs(x);

			if (a < DBL_EPSILON) return x;
			if (a > 0.697) return (Math.Exp(x) - 1);

			if (a > 1e-8) y = Math.Exp(x) - 1;
			else y = (x / 2 + 1) * x; /* Taylor expansion, more accurate in this range */

			/* Newton step for solving   log(1 + y) = x   for y : */
			/* WARNING: does not work for y ~ -1: bug in 1.5.0 */
			y -= (1 + y) * (log1p (y) - x);
			return y;
		}

		/** Method: 
		 Compute the relative error logarithm. log(1 + x).
		x - Value for which to calculate Math.Log (1 + x).*/
        internal double log1p(double x) {
			return (Math.Log (1 + x));
		}

		/** Method: 
		 Metodo interno necesario para calcular qchisq
		p - Probabilidad que se busca.
		nu - Parametro Media de la distribución Normal.
		g - Value for which to calculate Math.Log (1 + x).
		lower_tail - Si lower_tail = true devuelve la cola de la izquierda, sino la de la derecha.
		log_p - Si log_p = true, devuelve el logaritmo neperiano del quantil, sino el quantil.
		tol - Value for which to calculate Math.Log (1 + x).  */
        internal  double qchisq_appr(double p, double nu, double g/* = log Gamma(nu/2) */, bool lower_tail, bool log_p, double tol /* EPS1 */)	{
            const double C7 = 4.67;
            const double C8 = 6.66;
            const double C9 = 6.73;
            const double C10 = 13.32;
            const double M_LN2 = 0.693147180559945309417232121458;

            double p_, alpha, a, c, ch, p1;
            double p2, q, t, x;

            /* test arguments and initialise */

            if (Double.IsNaN(p) || Double.IsNaN(nu))
                return p + nu;

            if (R_Q_P01_check(p, log_p))
                throw new ArgumentException("Invalid arguments to the R qchisq appr function Invalid p and or log p");

            if (nu <= 0) throw new ArgumentException("Invalid arguments to the R qchisq appr function Invalid nu");

            p_ = R_DT_qIv(p, log_p, lower_tail); /* lower_tail prob (in any case) */

            alpha = 0.5 * nu;/* = [pq]gamma() shape */
            c = alpha - 1;


            if (nu < (-1.24) * (p1 = R_DT_log(p, log_p, lower_tail)))
                /* for small chi-squared */
                ch = Math.Exp((Math.Log(alpha) + p1 + g) / alpha + M_LN2);
            else if (nu > 0.32)
            {	/*  using Wilson and Hilferty estimate */

                x = qnorm(p, 0, 1, lower_tail, log_p);
                p1 = 2.0 / (9.0 * nu);
                ch = nu * Math.Pow(x * Math.Sqrt(p1) + 1 - p1, 3);

                /* approximation for p tending to 1: */
                if (ch > 2.2 * nu + 6)
                    ch = -2 * (R_DT_Clog(p, log_p, lower_tail) - c * Math.Log(0.5 * ch) + g);
            }
            else
            { /* "small nu" : 1.24*(-log(p)) <= nu <= 0.32 */

                ch = 0.4;
                a = R_DT_Clog(p, log_p, lower_tail) + g + c * M_LN2;
                do
                {
                    q = ch;
                    p1 = 1.0 / (1 + ch * (C7 + ch));
                    p2 = ch * (C9 + ch * (C8 + ch));
                    t = -0.5 + (C7 + 2 * ch) * p1 - (C9 + ch * (C10 + 3 * ch)) / p2;
                    ch -= (1 - Math.Exp(a + 0.5 * ch) * p2 * p1) / t;
                } while (Math.Abs(q - ch) > tol * Math.Abs(ch));
            }

            return ch;
		}

        /** Method:  Normal Distribution Function 
        x -  value to calculate probability
        m -  mean 
        s -  standard deviation */
        internal  double pnorm(double x, double m, double s) {
            return pnorm(x, m, s, true, false);
        }

        /** Method:   Normal Quantile Funciton  
        p -  probability 
        m -  mean 
        s -  standard deviation */
        internal double qnorm(double p, double m, double s) {
            return qnorm(p, m, s, true, false);
        }
		
        /** Method: 
		 Compute the quantile function for the normal distribution.
		 For small to moderate probabilities, algorithm referenced
		 below is used to obtain an initial approximation which is
		 polished with a final Newton step.
		 For very large arguments, an algorithm of Wichura is used.
		 Si lower_tail = true devuelve la cola de la izquierda, sino la de la derecha.
		 Si log_p = true, devuelve el logaritmo neperiano del la función de probabilidad, sino el quantil.
		x - Probabilidad que se busca.
		mu - Parametro Media de la distribución Normal.
		sigma - Parametro Varianza de la distribución Normal.
		lower_tail - Si lower_tail = true devuelve la cola de la izquierda, sino la de la derecha.
		log_p - Si log_p = true, devuelve el logaritmo neperiano del quantil, sino el quantil. */
        internal double pnorm(double x, double mu, double sigma, bool lower_tail, bool log_p) {
			double p, cp = 0.0;

			/* Note: The structure of these checks has been carefully thought through.
			 * For example, if x == mu and sigma == 0, we get the correct answer 1.
			 */
			if(Double.IsNaN (x) || Double.IsNaN (mu) || Double.IsNaN (sigma))
				return x + mu + sigma;
			if(!R_finite (x) && mu == x) return (0.0 / 0.0); /* x-mu is NaN */
			if (sigma <= 0.0) 
			{
				if (sigma < 0.0) return (0.0 / 0.0);
				/* sigma = 0 : */
				return (x < mu) ? R_DT_0 (lower_tail, log_p) : R_DT_1 (lower_tail, log_p);
			}
			p = (x - mu) / sigma;
			if(!R_finite (p))
				return (x < mu) ? R_DT_0 (lower_tail, log_p): R_DT_1 (lower_tail, log_p);
			x = p;

			pnorm_both(x, ref p, ref cp, lower_tail, log_p);

			return(lower_tail ? p : cp);
		} 

        private  void pnorm_both(double x, ref double cum, ref double ccum, bool lower_tail, bool log_p)	{
			/* i tail in {0,1,2} means: "lower", "upper", or "both" :
			   if(lower) return  *cum := P[X <= x]
			   if(upper) return *ccum := P[X >  x] = 1 - P[X <= sample]
			*/
			double [] a = 
							{
								2.2352520354606839287,
								161.02823106855587881,
								1067.6894854603709582,
								18154.981253343561249,
								0.065682337918207449113
							};
			double [] b = 
							{
								47.20258190468824187,
								976.09855173777669322,
								10260.932208618978205,
								45507.789335026729956
							};
			double [] c = 
							{
								0.39894151208813466764,
								8.8831497943883759412,
								93.506656132177855979,
								597.27027639480026226,
								2494.5375852903726711,
								6848.1904505362823326,
								11602.651437647350124,
								9842.7148383839780218,
								1.0765576773720192317e-8
							};
			double [] d = 
							{
								22.266688044328115691,
								235.38790178262499861,
								1519.377599407554805,
								6485.558298266760755,
								18615.571640885098091,
								34900.952721145977266,
								38912.003286093271411,
								19685.429676859990727
							};
			double [] p = 
							{
								0.21589853405795699,
								0.1274011611602473639,
								0.022235277870649807,
								0.001421619193227893466,
								2.9112874951168792e-5,
								0.02307344176494017303
							};
			double [] q = 
							{
								1.28426009614491121,
								0.468238212480865118,
								0.0659881378689285515,
								0.00378239633202758244,
								7.29751555083966205e-5
							};

			double xden, xnum, temp, del, eps, xsq, y;
			double min = DBL_MIN;
			int i;

			if(Double.IsNaN (x)) { cum = ccum = x; return; }

			/* Consider changing these : */
			eps = DBL_EPSILON * 0.5;

			/* i tail in {0,1,2} =^= {lower, upper, both} */
			/* lower = i tail != 1;
			   upper = i tail != 0; */

			y = Math.Abs (x);
			if (y <= 0.67448975) 
			{ /* qnorm(3/4) = .6744.... -- earlier had 0.66291 */
				if (y > eps) 
				{
					xsq = x * x;
					xnum = a[4] * xsq;
					xden = xsq;
					for (i = 0; i < 3; ++i) 
					{
						xnum = (xnum + a[i]) * xsq;
						xden = (xden + b[i]) * xsq;
					}
				} 
				else xnum = xden = 0.0;

				temp = x * (xnum + a[3]) / (xden + b[3]);
				if(lower_tail)  cum = 0.5 + temp;
				if(!lower_tail) ccum = 0.5 - temp;
				if(log_p) 
				{
					if(lower_tail)  cum = Math.Log(cum);
					if(!lower_tail) ccum = Math.Log(ccum);
				}
			}
			else if (y <= M_SQRT_32) 
			{

				/* Evaluate pnorm for 0.674.. = qnorm(3/4) < |x| <= sqrt(32) ~= 5.657 */

				xnum = c[8] * y;
				xden = y;
				for (i = 0; i < 7; ++i) 
				{
					xnum = (xnum + c[i]) * y;
					xden = (xden + d[i]) * y;
				}
				temp = (xnum + c[7]) / (xden + d[7]);

				/*
				#define do del(X)	
								xsq = Decimal.Truncate (X * SIXTEN) / SIXTEN;				
								del = (X - xsq) * (X + xsq);
								if(log_p) 
								{
									cum = (-xsq * xsq * 0.5) + (-del * 0.5) + Math.Log(temp);	
									if((lower_tail && x > 0.0) || (!lower_tail && sample <= 0.0))			
										ccum = log1p (-Math.Exp (-xsq * xsq * 0.5) *	Math.Exp (-del * 0.5) * temp);		
								}								
								else 
								{
									cum = Math.Exp (-xsq * xsq * 0.5) * Math.Exp (-del * 0.5) * temp;	
									ccum = 1.0 - cum;						
								}

				#define swap tail						
								if (x > 0.0) 
								{
									/ swap  ccum <--> cum
									temp = cum; if(lower) cum = ccum; ccum = temp;	
								}

				*/
				// Se substituye por do del (X)
			
				xsq = Trunc (y * SIXTEN) / SIXTEN;				
				del = (y - xsq) * (y + xsq);
				if(log_p) 
				{
					cum = (-xsq * xsq * 0.5) + (-del * 0.5) + Math.Log(temp);	
					if((lower_tail && x > 0.0) || (!lower_tail && x <= 0.0))			
						ccum = log1p (-Math.Exp (-xsq * xsq * 0.5) *	Math.Exp (-del * 0.5) * temp);		
				}								
				else 
				{
					cum = Math.Exp (-xsq * xsq * 0.5) * Math.Exp (-del * 0.5) * temp;	
					ccum = 1.0 - cum;						
				}

				// Se substituye por swap tail
				if (x > 0.0) 
				{
					temp = cum; if(lower_tail) cum = ccum; ccum = temp;	
				}
			}

				/* else	  |x| > sqrt(32) = 5.657 :
				 * the next two case differentiations were really for lower=T, log=F
				 * Particularly	 *not*	for  log_p !

				 * Cody had (-37.5193 < x  &&  x < 8.2924) ; R originally had y < 50
				 *
				 * Note that we do want symmetry(0), lower/upper -> hence use y
				 */
			else if(log_p
				/*  ^^^^^ MM FIXME: can speedup for log_p and much larger |x| !
				 * Then, make use of  Abramowitz & Stegun, 26.2.13, something like

				 xsq = x*x;

				 if(xsq * DBL_EPSILON < 1.)
					del = (1. - (1. - 5./(xsq+6.)) / (xsq+4.)) / (xsq+2.);
				 else
					del = 0.;
				 *cum = -.5*xsq - M LN SQRT 2PI - log(x) + log1p(-del);
				 *ccum = log1p(-exp(*cum)); /.* ~ log(1) = 0 *./

				 swap tail;

				*/
				|| (lower_tail && -37.5193 < x  &&  x < 8.2924)
				|| (!lower_tail && -8.2924  < x  &&  x < 37.5193)
				) 
			{

				/* Evaluate pnorm for x in (-37.5, -5.657) union (5.657, 37.5) */
				xsq = 1.0 / (x * x);
				xnum = p[5] * xsq;
				xden = xsq;
				for (i = 0; i < 4; ++i) 
				{
					xnum = (xnum + p[i]) * xsq;
					xden = (xden + q[i]) * xsq;
				}
				temp = xsq * (xnum + p[4]) / (xden + q[4]);
				temp = (M_1_SQRT_2PI - temp) / y;

				// Se substituye por do del (X)
			
				xsq = Trunc (x * SIXTEN) / SIXTEN;				
				del = (x - xsq) * (x + xsq);
				if(log_p) 
				{
					cum = (-xsq * xsq * 0.5) + (-del * 0.5) + Math.Log(temp);	
					if((lower_tail && x > 0.0) || (!lower_tail && x <= 0.0))			
						ccum = log1p (-Math.Exp (-xsq * xsq * 0.5) *	Math.Exp (-del * 0.5) * temp);		
				}								
				else 
				{
					cum = Math.Exp (-xsq * xsq * 0.5) * Math.Exp (-del * 0.5) * temp;	
					ccum = 1.0 - cum;						
				}

				// Se substituye por swap tail
				if (x > 0.0) 
				{
					temp = cum; if(lower_tail) cum = ccum; ccum = temp;	
				}
			}
			else 
			{ /* no log_p , large x such that probs are 0 or 1 */
				if(x > 0) {	cum = 1.0; ccum = 0.0;	}
				else {	        cum = 0.0; ccum = 1.0;	}
			}
			return;
		}

		/** Method: 
		 Compute the quantile function for the normal distribution. For small to moderate probabilities, algorithm referenced
		 below is used to obtain an initial approximation which is polished with a final Newton step.
		 For very large arguments, an algorithm of Wichura is used. Si lower_tail = true devuelve la cola de la izquierda, sino la de la derecha.
		 Si log_p = true, devuelve el logaritmo neperiano del la función de probabilidad, sino el quantil.
		 
		p - Probabilidad que se busca.
		mu - Parametro Media de la distribución Normal.
		sigma - Parametro Varianza de la distribución Normal.
		lower_tail - Si lower_tail = true devuelve la cola de la izquierda, sino la de la derecha.
		log_p - Si log_p = true, devuelve el logaritmo neperiano del quantil, sino el quantil. */
        internal double qnorm(double p, double mu, double sigma, bool lower_tail, bool log_p) {
            double p_, q, r, val;

            if (Double.IsNaN(p) || Double.IsNaN(mu) || Double.IsNaN(sigma))
                return p + mu + sigma;

            if (p == R_DT_0(lower_tail, log_p)) return ((-1.0) / 0.0);
            if (p == R_DT_1(lower_tail, log_p)) return (1.0 / 0.0);
            if (R_Q_P01_check(p, log_p))
                throw new ArgumentException("Invalid arguments to the R qnorm function Invalid p and or log p");

            if (sigma < 0) throw new ArgumentException("Invalid arguments to the R qnorm function Invalid sigma");
            if (sigma == 0) return mu;

            p_ = R_DT_qIv(p, log_p, lower_tail);/* real lower_tail prob. p */
            q = p_ - 0.5;

            /*-- use AS 241 --- */
            /* double ppnd16_(double *p, long *ifault)*/
            /*      ALGORITHM AS241  APPL. STATIST. (1988) VOL. 37, NO. 3

                    Produces the normal deviate Z corresponding to a given lower
                    tail area of P; Z is accurate to about 1 part in 10**16.

                    (original fortran code used PARAMETER(..) for the coefficients
                     and provided hash codes for checking them...)
            */
            if (Math.Abs(q) <= 0.425)
            {/* 0.075 <= p <= 0.925 */
                r = 0.180625 - q * q;
                val =
                    q * (((((((r * 2509.0809287301226727 +
                    33430.575583588128105) * r + 67265.770927008700853) * r +
                    45921.953931549871457) * r + 13731.693765509461125) * r +
                    1971.5909503065514427) * r + 133.14166789178437745) * r +
                    3.387132872796366608)
                    / (((((((r * 5226.495278852854561 +
                    28729.085735721942674) * r + 39307.89580009271061) * r +
                    21213.794301586595867) * r + 5394.1960214247511077) * r +
                    687.1870074920579083) * r + 42.313330701600911252) * r + 1.0);
            }
            else
            { /* closer than 0.075 from {0,1} boundary */

                /* r = min(p, 1-p) < 0.075 */
                if (q > 0.0)
                    r = R_DT_CIv(p, log_p, lower_tail); /* 1-p */
                else
                    r = p_;/* = R_DT_Iv(p) ^=  p */

                r = Math.Sqrt(-((log_p &&
                    ((lower_tail && q <= 0) || (!lower_tail && q > 0))) ?
                p : /* else */ Math.Log(r)));
                /* r = sqrt(-log(r))  <==>  min(p, 1-p) = exp( - r^2 ) */

                if (r <= 5.0)
                { /* <==> min(p,1-p) >= exp(-25) ~= 1.3888e-11 */
                    r += -1.6;
                    val = (((((((r * 7.7454501427834140764e-4 +
                        .0227238449892691845833) * r + .24178072517745061177) *
                        r + 1.27045825245236838258) * r +
                        3.64784832476320460504) * r + 5.7694972214606914055) *
                        r + 4.6303378461565452959) * r +
                        1.42343711074968357734)
                        / (((((((r *
                        1.05075007164441684324e-9 + 5.475938084995344946e-4) *
                        r + .0151986665636164571966) * r +
                        .14810397642748007459) * r + .68976733498510000455) *
                        r + 1.6763848301838038494) * r +
                        2.05319162663775882187) * r + 1.0);
                }
                else
                { /* very close to  0 or 1 */
                    r += -5.0;
                    val = (((((((r * 2.01033439929228813265e-7 +
                        2.71155556874348757815e-5) * r +
                        .0012426609473880784386) * r + .026532189526576123093) *
                        r + .29656057182850489123) * r +
                        1.7848265399172913358) * r + 5.4637849111641143699) *
                        r + 6.6579046435011037772)
                        / (((((((r *
                        2.04426310338993978564e-15 + 1.4215117583164458887e-7) *
                        r + 1.8463183175100546818e-5) * r +
                        7.868691311456132591e-4) * r + .0148753612908506148525)
                        * r + .13692988092273580531) * r +
                        .59983220655588793769) * r + 1.0);
                }

                if (q < 0.0)
                    val = -val;
                /* return (q >= 0.)? r : -r ;*/
            }
            return mu + sigma * val;
        }

		/** Method: 
		 This function computes the distribution function for the
			gamma distribution with shape parameter alph and scale parameter
		 scale.	This is also known as the incomplete gamma function.
		 See Abramowitz and Stegun (6.5.1) for example.
		 Si lower_tail = true devuelve la cola de la izquierda, sino la de la derecha.
		 Si log_p = true, devuelve el logaritmo neperiano del la función de probabilidad, sino el quantil.
		 Si se desea comparar con lo que devuelve R, entonces el parametro rate de R, 
		 debe ser siempre 0.
		x - Probabilidad que se busca.
		alph - Parametro alpha de la distribución gamma.
		scale - Parametro de escala de la distribución gamma.
		lower_tail - Si lower_tail = true devuelve la cola de la izquierda, sino la de la derecha.
		log_p - Si log_p = true, devuelve el logaritmo neperiano del quantil, sino el quantil.*/
        internal double pgamma(double x, double alph, double scale, bool lower_tail, bool log_p) {
			const double xbig = 1.0e+8, xlarge = 1.0e+37, alphlimit = 1e5;/* was 1000. till 1.8.x */
			double pn1, pn2, pn3, pn4, pn5, pn6, arg, a, b, c, an, osum, sum;
			long n;
			bool pearson;

			/* check that we have valid values for x and alph */

			if (Double.IsNaN (x) || Double.IsNaN (alph) || Double.IsNaN (scale)) return x + alph + scale;
			if (alph <= 0.0 || scale <= 0.0)
				throw new ArgumentException("Invalid arguments to the R pgamma function Invalid alpha o scale");

			x /= scale;

			if (Double.IsNaN (x)) /* eg. original x = scale = Inf */
				return x;

			if (x <= 0.0) return R_DT_0 (lower_tail, log_p);

			if (alph > alphlimit) 
			{ /* use a normal approximation */
				pn1 = Math.Sqrt(alph) * 3.0 * (Math.Pow (x / alph, 1.0 / 3.0) + 1.0 / (9.0 * alph) - 1.0); 
				return pnorm (pn1, 0.0, 1.0, lower_tail, log_p);
			}

			if (x > xbig * alph) 
			{
				if (x > DBL_MAX * alph)
					/* if x is extremely large   compared to alph   then return 1 */
					return R_DT_1 (lower_tail, log_p);
				else 
				{ /* this only "helps" when log_p = TRUE */
					pn1 = Math.Sqrt(alph) * 3.0 * (Math.Pow (x / alph, 1.0 / 3.0) + 1.0 / (9.0 * alph) - 1.0); 
					return pnorm (pn1, 0.0, 1.0, lower_tail, log_p);
				}
			}

			if (x <= 1.0 || x < alph) 
			{

				pearson = true;/* use pearson's series expansion. */

				arg = alph * Math.Log(x) - x - stat.lngamma (alph + 1.0);
				c = 1.0;
				sum = 1.0;
				a = alph;
				do 
				{
					a += 1.0;
					c *= x / a;
					sum += c;
				} while (c > DBL_EPSILON * sum);
			}
			else 
			{ /* x >= max( 1, alph) */

				pearson = false;/* use a continued fraction expansion */

				arg = alph * Math.Log(x) - x - stat.lngamma (alph);
				a = 1.0 - alph;
				b = a + x + 1.0;
				pn1 = 1.0;
				pn2 = x;
				pn3 = x + 1.0;
				pn4 = x * b;
				sum = pn3 / pn4;
				for (n = 1; ; n++) 
				{
					a += 1.0;/* =   n+1 -alph */
					b += 2.0;/* = 2(n+1)-alph+x */
					an = a * n;
					pn5 = b * pn3 - an * pn1;
					pn6 = b * pn4 - an * pn2;
					if (Math.Abs (pn6) > 0.0) 
					{
						osum = sum;
						sum = pn5 / pn6;
						if (Math.Abs(osum - sum) <= DBL_EPSILON * fmin2 (1.0, sum))
							break;
					}
					pn1 = pn3;
					pn2 = pn4;
					pn3 = pn5;
					pn4 = pn6;
					if (Math.Abs (pn5) >= xlarge) 
					{
						/* re-scale the terms in continued fraction if they are large */
						pn1 /= xlarge;
						pn2 /= xlarge;
						pn3 /= xlarge;
						pn4 /= xlarge;
					}
				}
			}

			arg += Math.Log(sum);

			// Original source code. I changed it because lower_tail is not int, but bool
			// 
			//           lower_tail = (lower_tail == pearson);
			// 
			// signal = ((pearson == 0) && (!lower_tail)) ||((pearson == 1) && (lower_tail));
			lower_tail = (lower_tail == pearson);
			// if (log_p && signal)
			if (log_p && lower_tail)
				return (arg);
			/* else */
			/* sum = exp(arg); and return   if(lower_tail) sum  else 1-sum : */
			return (lower_tail) ? Math.Exp(arg) : (log_p ? R_Log1_Exp (arg) : -expm1 (arg));
		}

		/** Method: 
		 Computes the density of the gamma distribution,
		 
		            1/s (x/s)^{a-1} exp(-sample/s)
		  (x;a,s) = -----------------------
		                     (a-1)!
		 
		  where `s' is the scale (= 1/lambda in other parametrizations)
		  and `a' is the shape parameter ( = alpha in other contexts).
		 
		 Si lower_tail = true devuelve la cola de la izquierda, sino la de la derecha.
		 Si log_p = true, devuelve el logaritmo neperiano del la función de probabilidad, sino el quantil.
		 Si se desea comparar con lo que devuelve R, entonces el parametro rate de R, 
		 debe ser siempre 0.
		x - Probabilidad que se busca.
		shape - Parametro shape de la distribución gamma.
		scale - Parametro de escala de la distribución gamma.
		give_log - Si give_log = true, devuelve el logaritmo neperiano del quantil, sino el quantil. */
        internal double dgamma(double x, double shape, double scale, bool give_log) {
			double pr;
			if (Double.IsNaN(x) || Double.IsNaN(shape) || Double.IsNaN(scale))
				return x + shape + scale;
			if (shape <= 0 || scale <= 0) 
				throw new ArgumentException("Invalid arguments to the R dgamma function Invalid alpha o scale");

			if (x < 0.0)
				return R_D_0 (give_log);
			if (x == 0.0) 
			{
				if (shape < 1) return (1.0 / 0.0);
				if (shape > 1) return R_D_0 (give_log);
				/* else */
				return give_log ? -Math.Log(scale) : 1 / scale;
			}

			if (shape < 1) 
			{
				pr = dpois_raw(shape, x / scale, give_log);
				return give_log ?  pr + Math.Log(shape/x) : pr * shape / x;
			}
			/* else  shape >= 1 */
			pr = dpois_raw (shape-1, x / scale, give_log);
			return give_log ? pr - Math.Log (scale) : pr / scale;
		}

		
        /** Method: 
        x -  independent variable 
        lambda -  lambda 
        give_log -  logarithm */
        internal double dpois_raw(double x, double lambda, bool give_log) {
			if (lambda == 0) return( (x == 0) ? R_D_1 (give_log) : R_D_0 (give_log));
			if (x == 0) return( R_D_exp (-lambda, give_log, x) );
			if (x < 0)  return( R_D_0 (give_log));

			return(R_D_fexp (M_2PI*x, -stat.stirlerr (x) - stat.bd0 (x, lambda), give_log ));
		}

		
        /** Method:  Función LambertW */
        internal double LambertW(double x) {
			int i;
			double p, e, t, w, eps = 4.0e-16;
			if (x < -0.36787944117144232159552377016146086)
                throw new ArgumentException("Invalid Arguments To The R");
			if (x == 0.0) return 0.0;
			if (x < 1.0)
			{
				p = Math.Sqrt (2.0 * (2.7182818284590452353602874713526625 * x + 1.0));
				w = -1.0 + p - p * p / 3.0 + 11.0 / 72.0 * p * p * p;
			}
			else
				w = Math.Log (x);
			if (x < 3.0) w-= Math.Log (w);
			for (i = 0; i < 20; i++)
			{
				e = Math.Exp (w);
				t = w * e -x;
				t /= e * (w + 1.0) - 0.5 * (w + 2.0) * t / (w + 1.0);
				w -= t;
				if (Math.Abs (t) < eps * (1.0 + Math.Abs (w))) return w;
			}
            throw new ArgumentException("The RLambertW Function Does Not Convergence");
		}

        #endregion Métodos internal

        #region Private Methods

        private  double R_D_0 (bool log_p)
		{
			return (log_p ? ((-1.0) / 0.0) : 0.0);
		}

		private  double R_D_1 (bool log_p)
		{
			return (log_p ? 0.0 : 1.0);
		}

		private  double R_DT_0 (bool lower_tail, bool log_p)
		{
			return (lower_tail ? R_D_0 (log_p) : R_D_1 (log_p));
		}

		private  double R_DT_1 (bool lower_tail, bool log_p)
		{
			return (lower_tail ? R_D_1 (log_p) : R_D_0 (log_p));
		}

		private  double R_DT_log (double p, bool log_p, bool lower_tail)
		{
			return ((lower_tail? R_D_log (p, log_p) : R_D_LExp(p, log_p)));
		}

		private  double R_D_LExp (double p, bool log_p)
		{
			return (log_p ? R_Log1_Exp (p) : log1p(-p));
		}

		private  double R_D_exp (double p, bool log_p, double x)
		{
			return (log_p	?  (p)	 : Math.Exp(x));
		}

		private  double R_DT_Clog (double p, bool log_p, bool lower_tail)
		{
			// Substituir R_DT_log(p) por lo que viene a continuación
			return (lower_tail? (log_p ? R_Log1_Exp(p) : log1p(-p)): R_D_log (p, log_p));			
		}

		private  double R_Log1_Exp (double p)
		{
			const double M_LN2 = 0.693147180559945309417232121458;
			return ((p) > -M_LN2 ? Math.Log(-expm1(p)) : log1p(-Math.Exp(p)));				
		}

		private  double R_D_log (double p, bool log_p)
		{
			return (log_p	?  (p)	 : Math.Log(p));				
		}

		private  double R_D_fexp (double p, double f, bool log_p)
		{
			return (log_p ? -0.5 * Math.Log(p)+(f) : Math.Exp (f) / Math.Sqrt(p));
		}

		private  bool R_Q_P01_check (double p, bool log_p)
		{
			return ((log_p && p > 0) || (!log_p && (p < 0 || p > 1)));
		}

		private  double R_DT_qIv (double p, bool log_p, bool lower_tail)
		{
			return (log_p ? (lower_tail ? Math.Exp(p) : - expm1(p)) : (lower_tail ? (p) : (1 - (p))));
		}

		private  double R_DT_CIv (double p, bool log_p, bool lower_tail)
		{
			return (log_p ? (lower_tail ? -expm1 (p) : Math.Exp(p)) : R_D_Cval(p, lower_tail));
		}

		private  double R_D_Cval (double p, bool lower_tail)
		{
			return (lower_tail ? (1 - (p)) : (p));
		}

		private  double fmin2(double x, double y)
		{
			if (Double.IsNaN (x) || Double.IsNaN(y))
				return x + y;
			return (x < y) ? x : y;
		}

		private  bool R_finite (double x)
		{
			return (!Double.IsNaN (x) && (x != (1.0 / 0.0)) && (x != ((-1.0) / 0.0)));
		}

		private  double Trunc(double x)
		{
			if(x >= 0) return Math.Floor (x);
			else return Math.Ceiling (x);
		}

		#endregion Métodos Privados
		
	}
}
