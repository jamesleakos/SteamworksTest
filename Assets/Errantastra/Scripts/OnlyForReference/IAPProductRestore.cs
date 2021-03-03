
using UnityEngine;

namespace Errantastra
{
    /// <summary>
    /// Simple script to handle restoring purchases on iOS. Restoring purchases is a
    /// requirement by Apple and your app will be rejected if you do not provide it.
    /// </summary>
    public class IAPProductRestore : MonoBehaviour
    {
        //only show the restore button on iOS
        void Start()
        {
            #if !UNITY_IPHONE
                gameObject.SetActive(false);
            #endif
        }


        /// <summary>
        /// Calls Unity IAPs RestoreTransactions method.
        /// It makes sense to add this to an UI button event.
        /// </summary>
        public void Restore()
        {
            #if UNITY_PURCHASING
            UnityIAPManager.RestoreTransactions();
            #endif
        }
    }
}
