using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRClient.Helpers
{
    public static class SignalRRequestHelper
    {
        // this encruption is key based currently not used as tags need single way encruption and that function is in SignalRclient class this encruption only workds server side no client side and encruption key should not be accessed client side due to security reasons.

        ////// comparison should be done with encrupted tag so there should only be need of GetEncruptedTag tab fucntion
        //public static string geencuptedtagfromnonencrupted(string keytodecrupt) // dont n
        //{
        //    string nonencruptedtag = "invalid key";
        //    if (!string.isnullorwhitespace(keytodecrupt) && keytodecrupt.length > 3)
        //    {
        //        nonencruptedtag = ai.signalrclient.encriptionanddecritption.ecryptionanddcryption.decryptstring(keytodecrupt, keytodecrupt);
        //    }
        //    return nonencruptedtag;
        //}
    }
}
