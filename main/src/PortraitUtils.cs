using System;

namespace Myriad; 

public class PortraitUtils {
    
     //WE NEED THIS IN MULTIPLE PLACES BECAUSE IDK WHY RAIN WORLD DOES THIS, PLS
    public static int GetPortraitIndex (string fileName) {
        int result = -1; //NOT A PORTRAIT

        string lowerName = fileName.ToLower();
        
        if (lowerName.StartsWith("multiplayerportrait")) {
            int pDigits = 1; //ACCOUNT FOR DOUBLE DIGIT PLAYER NUMBERS
            
            if (fileName.Length == 22 || fileName.IndexOf("-") == 22) pDigits = 2; //A DOUBLE DIGIT PLAYER NUMBER
            
            string substr1 = lowerName.Replace("multiplayerportrait", "").Substring(0, pDigits); //GETS THE PLAYER NUMBER
            string substr2 = lowerName.Replace("multiplayerportrait", "").Substring(pDigits); //THE REST OF THE NUMBERS
            
            result = Convert.ToInt32(substr1);
        }
        
        return result;
    }

    public static string AdjustedPortraitFile(string fileName) {

        string newFileName = fileName;
        string lowerName = fileName.ToLower();
        //FILE NAMES ARE NOT CASE SENSITIVE. BUT FSPRITE NAMES ARE :/ SO DON'T CHANGE THE CASE
        //logger.LogMessage("STARTING FILE: " + fileName);

        //We CANNOT do this for player 5 because MSC slugcats use "MultiplayerPortrait40-" for their canon untinted color portraits -_- thanks rainworld
        if (lowerName.StartsWith("multiplayerportrait4") && lowerName.Length > 21) {
            //We'll have to let PlayerJoinButton deal with this because this is a valid portrait file
            //Except for the vanilla slugs because of course it would :/
            if (lowerName.EndsWith("white") || lowerName.EndsWith("yellow") || lowerName.EndsWith("red")) {
                newFileName = fileName.Replace("4", "0");
            }
        }
        else if (fileName.StartsWith("MultiplayerPortrait")) {
            int pDigits = 1; //ACCOUNT FOR DOUBLE DIGIT PLAYER NUMBERS
            
            if (fileName.Length == 22 || fileName.IndexOf("-") == 22) pDigits = 2; //A DOUBLE DIGIT PLAYER NUMBER
            
            string substr1 = fileName.Replace("MultiplayerPortrait", "").Substring(0, pDigits); //GETS THE PLAYER NUMBER
            string substr2 = fileName.Replace("MultiplayerPortrait", "").Substring(pDigits); //THE REST OF THE NUMBERS
            
            //IF OUR PLAYER NUM IS HIGHER THAN EXPECTED, RETURN THE 4TH PLAYER IMAGE VERSION
            if (Convert.ToInt32(substr1) > 3) substr1 = "0";
            
            newFileName = "MultiplayerPortrait" + substr1 + substr2; //REBUILD IT
        } else if (lowerName.StartsWith("gamepad") && lowerName.Length == 8) {
            int playNum = int.Parse(lowerName.Replace("gamepad", "")); //GETS THE PLAYER NUMBER
            
            if (playNum > 4) newFileName = "GamepadAny"; //JUST A PLACEHOLDER
        }
        //logger.LogMessage("EDITED FILE: " + newFileName);

        return newFileName;
    }
}