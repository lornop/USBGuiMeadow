using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USBGuiMeadow
{
    class SolarCalc
    {
        //Field
        private static double ResistorValue;
        private const int numberOfSamples = 5;
        private static int currentIndex;
        private double[,] slidingWindowVoltage = new double[6, numberOfSamples];
        public double[] analogVoltage = new double[6];

        //Constructor that takes no argument
        public SolarCalc()
        {
            ResistorValue = 100.00;

        }

        //Method
        public void ParseSolarData(string newPacket)
        {
            for (int i = 0; i < 6; i++)
            {
                analogVoltage[i] = Convert.ToDouble(newPacket.Substring(6+(i*4), 4));
                analogVoltage[i] = averageVoltage(analogVoltage[i], i); 
            }
        }

        private double averageVoltage(double voltageToAverage, int indexOfAnalog)
        {
            double sum;
            if(currentIndex >= numberOfSamples)
            {
                currentIndex = 0;

            }

            slidingWindowVoltage[indexOfAnalog, currentIndex] = voltageToAverage;
            sum = 0;
            for (int i = 0; i < numberOfSamples; i++)
            {
                sum += slidingWindowVoltage[indexOfAnalog, i];

            }
            if (indexOfAnalog == 5)
            {
                currentIndex++;
            }

            return sum / numberOfSamples;

        }

        public string GetVoltage(double analogValue)
        {
            double dAnalogValue = analogValue / 1000.0;
            return dAnalogValue.ToString(" 0.0 V");

        }

        public string GetCurrent(double an1, double shuntResistorAnalog)
        {
            double shuntAnalog = an1 - shuntResistorAnalog;
            double dAnalog = (shuntAnalog / ResistorValue);
            
            //Return the value with formats when dAnalog is positive; negative; or zero
            return dAnalog.ToString(" 0.0 mA; -0.0 mA; 0.0 mA");
        }

        public string GetLEDCurrent(double an1, double shuntResistorAnalog)
        {
            double shuntAnalog = an1 - shuntResistorAnalog;
            double dAnalog = (shuntAnalog / ResistorValue);
            if (dAnalog < 0)
            {
                dAnalog = 0;
            }

            //Return the value with formats when dAnalog is positive; negative; or zero
            return dAnalog.ToString(" 0.0 mA; -0.0 mA; 0.0 mA");
        }
    }
}
