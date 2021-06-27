/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System;

namespace com.tibco.sample.authentication
{
    public class UserNamePasswordValidator : System.IdentityModel.Selectors.UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            string authMsg = "";

            if (Authenticate(userName, password, out authMsg))
            {
                // Do post authentication work here (i.e. add username, password to cache)
            }
            else
            {
                throw new System.IdentityModel.Tokens.SecurityTokenValidationException(authMsg);
            }
        }

        private bool Authenticate(string userName, string password, out string authMsg)
        {
            // Real authentication code goes here.
            bool authValid = true;

            authMsg = String.Format("Authentication {0}", authValid ? "Succeeded" : "Failed"); 
            return authValid;
        }
    }
}
