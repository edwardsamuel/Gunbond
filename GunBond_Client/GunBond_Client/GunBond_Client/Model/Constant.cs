using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GunBond_Client.Model
{
    public static class Constant
    {
        public const int MSG_HANDSHAKE = 1;
        public static readonly byte[] msg_handshake = new byte[20] 
                { (byte)'G', (byte)'u', (byte)'n', (byte)'b', (byte)'o',
                   (byte)'n', (byte)'d', (byte)'G', (byte)'a', (byte)'m',
                    (byte)'e', 0, 0, 0, 0, 0, 0, 0, 0, 135 };

        public const int MSG_KEEP_ALIVE = 2;
        public static readonly byte[] msg_keep_alive = new byte[20] 
                { (byte)'G', (byte)'u', (byte)'n', (byte)'b', (byte)'o',
                   (byte)'n', (byte)'d', (byte)'G', (byte)'a', (byte)'m',
                    (byte)'e', 0, 0, 0, 0, 0, 0, 0, 0, 132 };

        public const int MSG_CREATE = 3;
        public static readonly byte[] msg_create = new byte[20] 
                { (byte)'G', (byte)'u', (byte)'n', (byte)'b', (byte)'o',
                   (byte)'n', (byte)'d', (byte)'G', (byte)'a', (byte)'m',
                    (byte)'e', 0, 0, 0, 0, 0, 0, 0, 0, 255 };

        public const int MSG_LIST = 4;
        public static readonly byte[] msg_list = new byte[20] 
                { (byte)'G', (byte)'u', (byte)'n', (byte)'b', (byte)'o',
                   (byte)'n', (byte)'d', (byte)'G', (byte)'a', (byte)'m',
                    (byte)'e', 0, 0, 0, 0, 0, 0, 0, 0, 254 };

        public const int MSG_ROOM = 5;
        public static readonly byte[] msg_room = new byte[20] 
                { (byte)'G', (byte)'u', (byte)'n', (byte)'b', (byte)'o',
                   (byte)'n', (byte)'d', (byte)'G', (byte)'a', (byte)'m',
                    (byte)'e', 0, 0, 0, 0, 0, 0, 0, 0, 200 };

        public const int MSG_SUCCESS = 6;
        public static readonly byte[] msg_success = new byte[20] 
                { (byte)'G', (byte)'u', (byte)'n', (byte)'b', (byte)'o',
                   (byte)'n', (byte)'d', (byte)'G', (byte)'a', (byte)'m',
                    (byte)'e', 0, 0, 0, 0, 0, 0, 0, 0, 127 };

        public const int MSG_FAILED = 7;
        public static readonly byte[] msg_failed = new byte[20] 
                { (byte)'G', (byte)'u', (byte)'n', (byte)'b', (byte)'o',
                   (byte)'n', (byte)'d', (byte)'G', (byte)'a', (byte)'m',
                    (byte)'e', 0, 0, 0, 0, 0, 0, 0, 0, 128 };

        public const int MSG_JOIN = 8;
        public static readonly byte[] msg_join = new byte[20] 
                { (byte)'G', (byte)'u', (byte)'n', (byte)'b', (byte)'o',
                   (byte)'n', (byte)'d', (byte)'G', (byte)'a', (byte)'m',
                    (byte)'e', 0, 0, 0, 0, 0, 0, 0, 0, 253 };

        public const int MSG_START = 9;
        public static readonly byte[] msg_start = new byte[20] 
                { (byte)'G', (byte)'u', (byte)'n', (byte)'b', (byte)'o',
                   (byte)'n', (byte)'d', (byte)'G', (byte)'a', (byte)'m',
                    (byte)'e', 0, 0, 0, 0, 0, 0, 0, 0, 252 };

        public const int MSG_QUIT = 10;
        public static readonly byte[] msg_quit = new byte[20] 
                { (byte)'G', (byte)'u', (byte)'n', (byte)'b', (byte)'o',
                   (byte)'n', (byte)'d', (byte)'G', (byte)'a', (byte)'m',
                    (byte)'e', 0, 0, 0, 0, 0, 0, 0, 0, 235 };

        public const int MSG_ADD = 11;
        public static readonly byte[] msg_add = new byte[20] 
                { (byte)'G', (byte)'u', (byte)'n', (byte)'b', (byte)'o',
                   (byte)'n', (byte)'d', (byte)'G', (byte)'a', (byte)'m',
                    (byte)'e', 0, 0, 0, 0, 0, 0, 0, 0, 197 };

        public const int MSG_REMOVE = 12;
        public static readonly byte[] msg_remove = new byte[20] 
                { (byte)'G', (byte)'u', (byte)'n', (byte)'b', (byte)'o',
                   (byte)'n', (byte)'d', (byte)'G', (byte)'a', (byte)'m',
                    (byte)'e', 0, 0, 0, 0, 0, 0, 0, 0, 245 };

        public static int check_message(byte[] input)
        {
            if (compare_message(input, msg_handshake))
            {
                return MSG_HANDSHAKE;
            }
            else if (compare_message(input, msg_keep_alive))
            {
                return MSG_KEEP_ALIVE;
            }
            else if (compare_message(input, msg_create))
            {
                return MSG_CREATE;
            }
            else if (compare_message(input, msg_list))
            {
                return MSG_LIST;
            }
            else if (compare_message(input, msg_room))
            {
                return MSG_ROOM;
            }
            else if (compare_message(input, msg_success))
            {
                return MSG_SUCCESS;
            }
            else if (compare_message(input, msg_failed))
            {
                return MSG_FAILED;
            }
            else if (compare_message(input, msg_join))
            {
                return MSG_JOIN;
            }
            else if (compare_message(input, msg_start))
            {
                return MSG_START;
            }
            else if (compare_message(input, msg_quit))
            {
                return MSG_QUIT;
            }
            else if (compare_message(input, msg_add))
            {
                return MSG_ADD;
            }
            else
            {
                return 0;
            }
        }

        private static bool compare_message(byte[] msg1, byte[] msg2)
        {
            if (msg1.Length < 20)
            {
                return false;
            }
            else
            {
                bool check = true;
                int i = 0;

                while (i < 20)
                {
                    if (!msg2[i].Equals(msg1[i]))
                    {
                        check = false;
                    }
                    i++;
                }

                return check;
            }
        }

        public static bool compare_bytes_special(byte[] msg1, byte[] msg2, int offset, int length)
        {
            bool check = true;
            int i = 0;

            while (i < length)
            {
                if (!msg2[i].Equals(msg1[offset + i]))
                {
                    check = false;
                }
                i++;
            }

            return check;
        }
    }
}
