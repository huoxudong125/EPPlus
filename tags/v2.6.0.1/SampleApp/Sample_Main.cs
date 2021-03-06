/*******************************************************************************
 * You may amend and distribute as you like, but don't remove this header!
 * 
 * All rights reserved.
 * 
 * EPPlus is an Open Source project provided under the 
 * GNU General Public License (GPL) as published by the 
 * Free Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
 * 
 * EPPlus provides server-side generation of Excel 2007 spreadsheets.
 * See http://www.codeplex.com/EPPlus for details.
 *
 *
 * 
 * The GNU General Public License can be viewed at http://www.opensource.org/licenses/gpl-license.php
 * If you unfamiliar with this license or have questions about it, here is an http://www.gnu.org/licenses/gpl-faq.html
 * 
 * The code for this project may be used and redistributed by any means PROVIDING it is 
 * not sold for profit without the author's written consent, and providing that this notice 
 * and the author's name and all copyright notices remain intact.
 * 
 * All code and executables are provided "as is" with no warranty either express or implied. 
 * The author accepts no liability for any damage or loss of business that this product may cause.
 *
 *
 * Code change notes:
 * 
 * Author							Change						Date
 *******************************************************************************
 * Jan K�llman		Added		10-SEP-2009
 *******************************************************************************/
using System;
using System.IO;
using OfficeOpenXml;

namespace EPPlusSamples
{
	class Sample_Main
	{
		static void Main(string[] args)
		{
			try
			{
                //Sample 3 and 4 uses the Adventureworks database. Enter then name of your SQL server into the variable below...
                string SqlServerName = "";

                // change this line to contain the path to the output folder
				DirectoryInfo outputDir = new DirectoryInfo(@"c:\temp\SampleApp");

				if (!outputDir.Exists) throw new Exception("outputDir does not exist!");
                //if (!templateDir.Exists) throw new Exception("templateDir does not exist!");
                
                // Sample 1 - simply creates a new workbook from scratch
				// containing a worksheet that adds a few numbers together 
				Console.WriteLine("Running sample 1");
				string output = Sample1.RunSample1(outputDir);
				Console.WriteLine("Sample 1 created: {0}", output);
				Console.WriteLine();

				// Sample 2 - simply reads some values from the file generated by sample 1
				// and outputs them to the console
				Console.WriteLine("Running sample 2");
				Sample2.RunSample2(output);
				Console.WriteLine();
				output = "";

                if (SqlServerName != "")
                {
                    // Sample 3 - creates a workbook from scratch and 
                    // populates using data from the AdventureWorks database
                    // This sample requires the AdventureWorks database.  
                    string connectionStr = string.Format(@"server={0};database=AdventureWorks;Integrated Security=true;", SqlServerName);
                    Console.WriteLine("Running sample 3");
                    output = Sample3.RunSample3(outputDir, connectionStr);
                    Console.WriteLine("Sample 3 created: {0}", output);
                    Console.WriteLine();

                    // Sample 4 - creates a workbook based on a template to demonstrat the use of a chart and 
                    // populates using data from the AdventureWorks database
                    // This sample requires the AdventureWorks database.  
                    Console.WriteLine("Running sample 4");
                    output = Sample4.RunSample4(connectionStr, new FileInfo("..\\..\\GraphTemplate.xlsx"), outputDir);      //Template path from /bin/debug or /bin/release
                    Console.WriteLine("Sample 4 created: {0}", output);
                    Console.WriteLine();
                }

                //Sample 5
                //Open sample 1 and adds a pie chart.
                Console.WriteLine("Running sample 5");
                Sample5.RunSample5(outputDir);
                Console.WriteLine("Sample 5 created:", output);
                Console.WriteLine();

                //Sample 6
                //Makes an advanced report on a directory in the filesystem.
                //Parameter 2 is the directory to report. Paramter 3 is how deep the scan will go. Parameter 4 Skips Icons if true (Set this to true to improve performance)
                //
                //This example demonstrates how to use outlines, shapes, pictures and charts.                
                Console.WriteLine("Running sample 6");
                Sample6.RunSample6(outputDir, new DirectoryInfo("..\\.."), 5, false);
                Console.WriteLine("Sample 6 created:", output);
                Console.WriteLine();

                //Sample 7
                //This sample shows the performance capabilities of the component and shows sheet protection.
                //Load X(param 2) rows with five columns
                Console.WriteLine("Running sample 7");
                Sample7.RunSample7(outputDir, 65534);
                Console.WriteLine("Sample 7 created:", output);
                Console.WriteLine();
            }
			catch (Exception ex)
			
            {
				
                Console.WriteLine("Error: {0}", ex.Message);
			}
			Console.WriteLine();
			Console.WriteLine("Press the return key to exit...");
			Console.Read();

		}
	}
}
