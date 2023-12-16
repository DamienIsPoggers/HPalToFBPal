using System.IO;

namespace HPalToFBPal
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 3)
            {
                Console.WriteLine("Args incorectly formatted.\n" +
                    "Correct formatting: \"HPalToFBPal.exe [hpalFile] [fbPalFile] [fbPalIndex] [optionals]\"\n" +
                    "Optional params: \n\t -reverse: Reverses the palette colors. \n\t -direction D: D sets what direction. 0 for both, 1 for right, 2 for left." +
                    "\n\t -3ds: Use if the hpal file comes from a 3ds arcsys game. \n\t -palSize S: S sets the size of the palette. Max of 256");
                goto End;
            }

            string hpalFilePath = args[0];
            string fbPalFilePath = args[1];
            int fbPalIndex = int.Parse(args[2]);

            bool reverse = false;
            byte direction = 0;
            bool _3ds = false;
            int paletteSize = 256;
            
            for(int i = 3; i < args.Length; i++)
                switch(args[i])
                {
                    default:
                        Console.WriteLine("Unknown arg: " + args[i] + ", skipping.");
                        break;
                    case "-reverse":
                        reverse = true;
                        break;
                    case "-direction":
                        byte.TryParse(args[i + 1], out direction);
                        i++;
                        if(direction > 2)
                        {
                            Console.WriteLine("Direction parameter invalid. Must be 0 for both, 1 for right, or 2 for left. Setting to 0");
                            direction = 0;
                        }    
                        break;
                    case "-3ds":
                        _3ds = true;
                        break;
                    case "-palSize":
                        paletteSize = int.Parse(args[i + 1]);
                        i++;
                        if (paletteSize > 256 || paletteSize < 1)
                        {
                            Console.WriteLine("Invalid palette size, reverting to default size.");
                            paletteSize = 256;
                        }
                        break;
                }

            Color[] palette = new Color[paletteSize];
            if(!File.Exists(hpalFilePath))
            {
                Console.WriteLine("HPal file input doesn't exist, ending.");
                goto End;
            }

            BinaryReader hpalFile = new BinaryReader(File.Open(hpalFilePath, FileMode.Open));
            if (_3ds)
                hpalFile.ReadBytes(16); //skip the header we dont care
            else
                hpalFile.ReadBytes(32);

            for(int i = 0; i < paletteSize; i++)
                palette[i] = new Color(hpalFile.ReadInt32());

            hpalFile.Close();
            hpalFile.Dispose();

            if(!File.Exists(fbPalFilePath))
            {
                Console.WriteLine("FBPal file input doesn't exist, ending.");
                goto End;
            }    

            BinaryReader fbPalFile = new BinaryReader(File.Open(fbPalFilePath, FileMode.Open));
            fbPalFile.ReadBytes(12); //we dont care about this part of the header
            int fbPalCount = fbPalFile.ReadInt32() / 2;
            if(fbPalIndex + 1 >= fbPalCount)
            {
                Console.WriteLine("Palette index given is bigger than FBPal palette count. Remember that it is 0 indexed. Exiting");
                goto End;
            }

            if (reverse)
                palette.Reverse();

            switch(direction)
            {
                default:
                case 0: //write to both sides
                    fbPalFile.ReadBytes(256 * 4 * fbPalIndex); //go to palette index
                    writeToFBPal(fbPalFile, palette); //write to right palette
                    fbPalFile.ReadBytes(256 * 4 * fbPalCount); //go to left palette
                    fbPalFile.BaseStream.Position -= paletteSize * 4; //move to the start of the palette
                    writeToFBPal(fbPalFile, palette); //write to left palette
                    break;
                case 1: //write to right side
                    fbPalFile.ReadBytes(256 * 4 * fbPalIndex); //go to palette index
                    writeToFBPal(fbPalFile, palette); //write
                    break;
                case 2: //write to left side
                    fbPalFile.ReadBytes(256 * 4 * fbPalCount); //go to start of left palettes
                    fbPalFile.ReadBytes(256 * 4 * fbPalIndex); //go to palette index
                    writeToFBPal(fbPalFile, palette); //write
                    break;
            }

            fbPalFile.Close();
            fbPalFile.Dispose();

            Console.WriteLine("Completed Successfully!");

        End:
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        static void writeToFBPal(BinaryReader fbPalFile, Color[] palette)
        {
            foreach (Color color in palette)
                fbPalFile.BaseStream.Write(BitConverter.GetBytes(color.rgba), 0, 4); //prolly a easier way of doing this but whatever
        }
    }

    public struct Color
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;

        public int rgba { get { return BitConverter.ToInt32(new byte[] { r, g, b, a }); } } //i could prolly write this better but idc it works

        public Color(int colorBgra)
        {
            byte[] bytes = BitConverter.GetBytes(colorBgra);
            b = bytes[0];
            g = bytes[1];
            r = bytes[2];
            a = bytes[3];
        }
        
        public Color(byte R, byte G, byte B, byte A)
        {
            r = R;
            g = G;
            b = B;
            a = A;
        }
    }
}
