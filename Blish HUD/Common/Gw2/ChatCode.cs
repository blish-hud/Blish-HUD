using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Blish_HUD.Common.Gw2
{
    public static class ChatCode
    {
        private static Logger Logger = Logger.GetLogger(typeof(ChatCode));

        /// <author>The original code was made in a JSFiddle by the GW2 Wikiuser Fam. Translation to C# by Andy</author>
        /// <seealso cref="https://jsfiddle.net/fffam/cg3njdu6/"/>
        /// <seealso cref="https://wiki.guildwars2.com/wiki/Talk:Chat_link_format#Quick_app_for_generating_item_codes"/>
        /// <summary>
        /// Decodes a Guild Wars 2 chat code.
        /// </summary>
        /// <param name="fullcode">This needs to be the item code or chat code (i.e. from trading post or gw2spidy) not the wardrobe ID</param>
        /// <returns>Item ID or 0, if invalid.</returns>
        public static int DecodeChatCode(string fullcode)
        {
            if (!Regex.IsMatch(fullcode, @"^\[\&")) {
                return 0;
            }
            var code = Regex.Replace(fullcode, @"^\[\&+|\]+$", "");
            var binary = Convert.FromBase64String(code);
            var octets = new char[binary.Length];
            for (var i = 0; i < binary.Length; i++)
            {
                octets[i] = (char)binary[i];
            }
            if (octets != null) {
                if (octets[0] == 2) {
                     return (octets[2] * 1)
                            +(octets[3] << 8)
                            +(octets[4] != null ? (octets[4] << 16) : 0);

                } else if (octets[0] == 11 ) {

                    return (octets[1] * 1)
                            +(octets[2] << 8)
                            +(octets[3] != null ? (octets[4] << 16) : 0);            
                } else {
                    Logger.Warn(fullcode + " must be a valid chat code");   
                }
            }
            return 0;
        }

        /// <author>The original code was made in a JSFiddle by the GW2 Wikiuser Fam. Translation to C# by Andy</author>
        /// <seealso cref="https://jsfiddle.net/fffam/cg3njdu6/"/>
        /// <seealso cref="https://wiki.guildwars2.com/wiki/Talk:Chat_link_format#Quick_app_for_generating_item_codes"/>
        /// <summary>
        /// Generates a chat code with the given parameters.
        /// </summary>
        /// <param name="itemId">The item identifier to generate a chat code for.</param>
        /// <param name="quantity">Optional: Quantity of the item.</param>
        /// <param name="upgrade1Id">Optional: An upgrade id if item has one upgrade slot.</param>
        /// <param name="upgrade2Id">Optional: An upgrade id if item has two upgrade slots.</param>
        /// <param name="skinId">Optional: A skin id if item is transmutable.</param>
        /// <returns>A Guild Wars 2 chat code</returns>
        public static string GenerateChatCode(int itemId, int quantity = 1, int upgrade1Id = 0, int upgrade2Id = 0, int skinId = 0)
        {

            // Figure out which header we need based on what components
            //0x00 – Default item
            //0x40 – 1 upgrade component
            //0x60 – 2 upgrade components
            //0x80 – Skinned
            //0xC0 – Skinned + 1 upgrade component
            //0xE0 – Skinned + 2 upgrade components
            var separator = 16 * ((skinId > 0 ? 8 : 0) + (upgrade1Id > 0 ? 4 : 0) + (upgrade2Id > 0 ? 2 : 0));

            // Arrange the IDs in order
            var ids = new int[] { 2, quantity % 256, itemId, separator, skinId, upgrade1Id, upgrade2Id };

            // Byte length for each part
            var lengths = new int[] { 1, 1, 3, 1, skinId > 0 ? 4 : 0, upgrade1Id > 0 ? 4 : 0, upgrade2Id > 0 ? 4 : 0 };

            // Build
            var bytes = new List<byte>();
            for (int i = 0; i < ids.Length; i++)
            {
                for (int j = 0; j < lengths[i]; j++)
                {
                    bytes.Add((byte)(ids[i] >> (8 * j) & 0xff));
                }
            }
            // Get code
            var output = Convert.ToBase64String(bytes.ToArray());
            return "[&" + output + "]";
        }
    }
}