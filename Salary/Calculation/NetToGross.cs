using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;

/* NetToGross
 * Class NetToGross gets net salary 
 * and returns dictionary with detailed 
 * info about salary (gross, tax excempt incom, etc)
 * (key - tax name, value - tax/salary)
 */


namespace Wages.Calculation
{
    public class NetToGross
    {
        public Dictionary<string, string> Calculator(double net)
        {
            //Dictionary for detailed salary info
            Dictionary<string, string> salaryInfo = new Dictionary<string, string>();

            //Variables for salaries and taxes
            var gross = 0.0;
            var taxExemptIncome = 0.0;
            var residentIncomeTax = 0.0;
            var healthInsurance = 0.0;
            var pensionSocialInsurance = 0.0;


            /* There are four conditions for four different scales:
             * If net < 282.1 (gross < 310)
             * If 282.1 <= net < 335.3 (310 <= gross < 380)
             * If 335.3 <= net < 760 (380 <= gross < 1000)
             * If net >= 760 (gross >= 1000) 
             */


            if (net < 282.1)
            {
                /* Extended: net salary divided by 91 %
                 * 100% - 9% (health (6%) + pesion&social(3%) insurances))
                 */
                gross = Math.Round((net / 0.91), 2);
                taxExemptIncome = 310.00;
            }
            else if (net >= 282.1 && net < 335.3)
            {
                /* Extended: 
                 * gross - > x
                 * net =  0.91x - 0.15(x - 310)
                 * */
                gross = Math.Round((net - 46.5) / 0.76, 2);
                taxExemptIncome = 310.00;
                residentIncomeTax = Math.Round((gross - taxExemptIncome) * 0.15, 2);

            }
            else if (net >= 335.3 && net < 760)
            {
                /* Extended:
                 * gross -> x
                 * net = 0.91x - 0.15(0.5(x - 380) + x - 310)
                 */
                gross = Math.Round((net - 75) / 0.685, 2);
                taxExemptIncome = Math.Round(310 - (0.5 * (gross - 380)), 2);
                residentIncomeTax = Math.Round((gross - taxExemptIncome) * 0.15, 2);
            }
            else
            {
                /* Extended: net salary divided by 91 %
                 * 100% - 15% (taxExcemptIncome) - 9% (health (6%) + pesion&social(3%) insurances)
                 */
                gross = Math.Round(net / 0.76, 2);
                residentIncomeTax = Math.Round(gross * 0.15, 2);
            }

            //HealtInsurance and PensionSocialInsurance is the same for all scales
            healthInsurance = Math.Round((gross * 0.06), 2);
            pensionSocialInsurance = Math.Round((gross * 0.03), 2);

            // Formating doubles to strings and adding to dictionary
            salaryInfo.Add("net",
                            string.Format("{0:N}", net));
            salaryInfo.Add("taxExemptIncome",
                            string.Format("{0:N}", taxExemptIncome));
            salaryInfo.Add("residentIncomeTax",
                            string.Format("{0:N}", residentIncomeTax));
            salaryInfo.Add("healthInsurance",
                            string.Format("{0:N}", healthInsurance));
            salaryInfo.Add("pensionSocialInsurance",
                            string.Format("{0:N}", pensionSocialInsurance));
            salaryInfo.Add("gross",
                            string.Format("{0:N}", gross));

            return salaryInfo;
        }
    }
}