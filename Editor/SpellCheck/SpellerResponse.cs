using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages.Excursion360_Builder.Editor.SpellCheck
{
    [Serializable]
    public class ResponseArrayWrapper
    {
        public RowResponse[] responses;
    }

    public enum ErrorCode
    {
        UnknownWord = 1,
        RepeatWord = 2,
        Capitalization = 3,
        TooManyErrors = 4
    }
    [Serializable]
    public class RowResponse
    {
        /// <summary>
        /// Error code from <see href="https://yandex.ru/dev/speller/doc/dg/reference/error-codes.html"/> 
        /// </summary>
        public ErrorCode code;
        /// <summary>
        /// Incorrect word start position
        /// </summary>
        public int pos;
        /// <summary>
        /// Incorrect word row number
        /// </summary>
        public int row;
        /// <summary>
        /// Incorrect word row start column
        /// </summary>
        public int col;
        /// <summary>
        /// Incorrect word length
        /// </summary>
        public int len;
        /// <summary>
        /// Incorrect word
        /// </summary>
        public string word;
        /// <summary>
        /// Examples to replace
        /// </summary>
        public string[] s;
    }

}
