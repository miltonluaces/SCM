#region Imports

using System;

#endregion

// Contains conversion support elements such as classes, interfaces and static methods.
// In order to convert some functionality to Visual C#, the Java Language Conversion Assistant creates "support classes" that duplicate the original functionality.  
// Support classes replicate the functionality of the original code, but in some cases they are substantially different architecturally. Although every effort is made to preserve the 
// original architecture of the application in the converted project, the user should be aware that the primary goal of these support classes is to replicate functionality, and that at times the architecture of the resulting solution may differ somewhat.
internal class SupportClass  {

	//Provides access to a static System.Random class instance
	static internal System.Random Random = new System.Random();

	// The class performs token processing in strings
	internal class Tokenizer: System.Collections.IEnumerator  {

        #region Fields

        // Position over the string
		private long currentPos = 0;
		// Include demiliters in the results.
		private bool includeDelims = false;
		// Char representation of the String to tokenize.
		private char[] chars = null;
		//The tokenizer uses the default delimiter set: the space character, the tab character, the newline character, and the carriage-return character and the form-feed character
		private string delimiters = " \t\n\r\f";

        #endregion

        #region Constructors

        // Initializes a new class instance with a specified string to process. "source" : String to tokenize
		internal Tokenizer(System.String source) {			
			this.chars = source.ToCharArray();
		}

		// Initializes a new class instance with a specified string to process and the specified token delimiters to use
		internal Tokenizer(System.String source, System.String delimiters):this(source) {			
			this.delimiters = delimiters;
		}


		// Initializes a new class instance with a specified string to process, the specified token delimiters to use, and whether the delimiters must be included in the results.
		internal Tokenizer(System.String source, System.String delimiters, bool includeDelims):this(source,delimiters) {
			this.includeDelims = includeDelims;
		}

        #endregion 

        #region Properties

        // Remaining tokens count
        public int Count
        {
            get
            {
                //keeping the current pos
                long pos = this.currentPos;
                int i = 0;

                try
                {
                    while (true)
                    {
                        this.NextToken();
                        i++;
                    }
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    this.currentPos = pos;
                    return i;
                }
            }
        }

        //  Performs the same action as NextToken.
        public System.Object Current
        {
            get { return (Object)this.NextToken(); }
        }

        #endregion

        #region Internal Methods

        // Returns the next token from the token list
		internal System.String NextToken()	{				
			return NextToken(this.delimiters);
		}

		// Returns the next token from the source string, using the provided token delimiters
		internal System.String NextToken(System.String delimiters) {
			//According to documentation, the usage of the received delimiters should be temporary (only for this call).
			//However, it seems it is not true, so the following line is necessary.
			this.delimiters = delimiters;

			//at the end 
			if (this.currentPos == this.chars.Length)
				throw new System.ArgumentOutOfRangeException();
			//if over a delimiter and delimiters must be returned
			else if (   (System.Array.IndexOf(delimiters.ToCharArray(),chars[this.currentPos]) != -1)
				     && this.includeDelims )                	
				return "" + this.chars[this.currentPos++];
			//need to get the token wo delimiters.
			else
				return nextToken(delimiters.ToCharArray());
		}

		//Returns the nextToken wo delimiters
		private System.String nextToken(char[] delimiters) {
			string token="";
			long pos = this.currentPos;

			//skip possible delimiters
			while (System.Array.IndexOf(delimiters,this.chars[currentPos]) != -1)
				//The last one is a delimiter (i.e there is no more tokens)
				if (++this.currentPos == this.chars.Length)
				{
					this.currentPos = pos;
					throw new System.ArgumentOutOfRangeException();
				}
			
			//getting the token
			while (System.Array.IndexOf(delimiters,this.chars[this.currentPos]) == -1)
			{
				token+=this.chars[this.currentPos];
				//the last one is not a delimiter
				if (++this.currentPos == this.chars.Length)
					break;
			}
			return token;
		}

				
		// Determines if there are more tokens to return from the source string. 
		internal bool HasMoreTokens() {
			//keeping the current pos
			long pos = this.currentPos;
			
			try
			{
				this.NextToken();
			}
			catch (System.ArgumentOutOfRangeException)
			{				
				return false;
			}
			finally
			{
				this.currentPos = pos;
			}
			return true;
		}

        //  Performs the same action as HasMoreTokens.
		public bool MoveNext()	{
			return this.HasMoreTokens();
		}
		
		/// Does nothing.
		public void  Reset() {
			;
        }

        #endregion
    }
}
