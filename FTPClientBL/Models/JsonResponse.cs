using System.Collections.Generic;

namespace FTPClient.BL.Model
{
    public class JsonResponse
    {
        private List<Error> errorList = new List<Error>();

        public JsonResponse()
        {
            this.Result = "error";

            this.ResponseData = new Dictionary<string, string>();
        }

        public string Result
        {
            get;
            set;
        }

        public Dictionary<string, string> ResponseData
        {
            get;
            set;
        }

        public List<Error> ErrorList
        {
            get
            {
                return this.errorList;
            }
            set
            {
                this.errorList = value;
            }
        }

        public bool RequestReload()
        {
            this.Result = "reload";

            this.ErrorList.Add(new Error
            {
                Point = null,
                Message = "ページを更新してください。"
            });

            return true;
        }

        public bool AddErrorInfo(string point, string errorMessage)
        {
            this.ErrorList.Add(new Error
            {
                Point = point,
                Message = errorMessage
            });

            return true;
        }

        public bool AddData(string key, string value)
        {
            this.ResponseData[key] = value;

            return true;
        }

        public bool SetResult(bool result)
        {
            if (result == true)
            {
                this.Result = "success";
            }
            else
            {
                this.Result = "error";
            }

            return true;
        }

        public class Error
        {
            public string Point
            {
                get;
                set;
            }

            public string Message
            {
                get;
                set;
            }
        }
    }
}