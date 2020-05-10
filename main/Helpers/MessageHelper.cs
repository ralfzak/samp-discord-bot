namespace main.Helpers
{
    public static class MessageHelper
    {
        public const string USER_NOT_FOUND = "User not found.";
        public const string COMMAND_SERVER_ONLY = "This only works on the server.";
        public const string NO_REASON_GIVEN = "No Reason Given";

        public static string GetVerificationCmdDescription(string mention)
        {
            return $"Hi {mention}! The verification process is used to claim your ownership of a SA:MP forum account. Provide your forum profile ID to start the process." +
                   "\n" +
                   "To initiate the process type `/verify <profile id>`" +
                   "\n" +
                   "Your profile ID is part of your profile link: https://i.imgur.com/1FEvYkj.png" +
                   "\n" +
                   "\n" +
                   "**EXAMPLE OF USAGE**" +
                   "\n" +
                   $"Lets assume your profile URL is *<{Program.FORUM_PROFILE_URL}218502>*" +
                   "\n" +
                   "Then your profile ID will be *218502*" +
                   "\n" +
                   "To start the verification process, type: `/verify 218502`" +
                   "\n" +
                   "Once you initiate the process, you will receive a token that you need to place in your biography section on your profile." +
                   "\n" +
                   "Once you place your token in the biography section, type `/verify done` so I can check your profile and verify you!" +
                   "\n" +
                   "You may cancel this process anytime by typing `/verify cancel`." +
                   "\n" +
                   "\n" +
                   "Facing trouble? Ask for help on Discord!";
        }

        public static string GetVerificationWaitingMessage(string mention, int profileid, string token)
        {
            return $"Thanks {mention}! The verification process has been initiated. All you have to do now is to paste the below token in your profile biography and type `/verify done` so I can have a quick look over it to verify you." +
                   "\n" +
                   "\n" +
                   $"Profile URL: {Program.FORUM_PROFILE_URL}{profileid}" +
                   "\n" +
                   $"Token: **{token}**" +
                   "\n" +
                   "\n" +
                   $"You can edit profile biography here: <http://forum.sa-mp.com/profile.php?do=editprofile> in the 'Additional Information' section at the end of the page." +
                   "\n" +
                   $"Please make sure your \"About Me\" profile section is visible to *Everyone* so I can see it.  You can change this setting here: <http://forum.sa-mp.com/profile.php?do=privacy>" +
                   "\n" +
                   "\n" +
                   "Facing trouble? Ask for help on Discord!";
        }

        public static string GetVerificationSuccessMessage(string mention, int profileid)
        {
            return $"Good work {mention}! You have completed your verification process and now verified on the SAMP discord server." +
                   "\n" +
                   "\n" +
                   $"Linked Forum Profile: {Program.FORUM_PROFILE_URL}{profileid}" +
                   "\n" +
                   "\n" +
                   "Congratulations. It was pleasure doing business with you." +
                   "\n" +
                   "Have a great day!";
        }
    }
}