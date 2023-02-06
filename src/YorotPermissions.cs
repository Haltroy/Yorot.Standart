using System.Threading.Tasks;

namespace Yorot
{
    /// <summary>
    /// Yorot Permission item
    /// </summary>
    public class YorotPermission
    {
        internal YorotPermissionMode allowance = YorotPermissionMode.None;

        /// <summary>
        /// Creates anew <see cref="YorotPermission"/>.
        /// </summary>
        /// <param name="id">ID of the permission.</param>
        /// <param name="requestor">The <see cref="object"/> that requested the permission.</param>
        /// <param name="main"><see cref="YorotMain"/></param>
        /// <param name="allowance">Permission mode.</param>
        public YorotPermission(string id, object requestor, YorotMain main, YorotPermissionMode _allowance = YorotPermissionMode.None)
        {
            ID = id;
            Requestor = requestor;
            Main = main;
            allowance = _allowance;
        }

        /// <summary>
        /// Id or internal name of this permission item.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The object that requested this permission.
        /// </summary>
        public object Requestor { get; set; }

        /// <summary>
        /// Main of this permmission.
        /// </summary>
        public YorotMain Main { get; set; }

        /// <summary>
        /// Value of this permission item.
        /// </summary>
        public YorotPermissionMode Allowance
        {
            get => allowance;
            set
            {
                Task.Run(async () =>
                {
                    var result = await Main.OnPermissionRequest(this, value);
                    allowance = result;
                });
            }
        }
    }

    /// <summary>
    /// Types of permission allowance.
    /// </summary>
    public enum YorotPermissionMode
    {
        /// <summary>
        /// Nothing set.
        /// </summary>
        None,

        /// <summary>
        /// Permission denied forever.
        /// </summary>
        Deny,

        /// <summary>
        /// Permission allowed forever.
        /// </summary>
        Allow,

        /// <summary>
        /// Permission allowed for one time only. Will reset to <see cref="None"/> on close.
        /// </summary>
        AllowOneTime
    }
}