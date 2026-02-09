using Plankton;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALDGP
{
    public class MeshIO
    {
        public static void WriteObj(PlanktonMesh mesh, string path)
        {
            StreamWriter sw = new StreamWriter(path);
            sw.WriteLine("# OBJ file written by ALG");
            var ci = System.Globalization.CultureInfo.InvariantCulture;

            sw.WriteLine("# " + mesh.Vertices.Count.ToString(ci) + " vertices");

            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                var v = mesh.Vertices[i];
                sw.Write("v ");
                sw.Write(v.X.ToString("r", ci));
                sw.Write(" ");
                sw.Write(v.Y.ToString("r", ci));
                sw.Write(" ");
                sw.WriteLine(v.Z.ToString("r", ci));
            }

            sw.WriteLine("# " + mesh.Faces.Count.ToString(ci) + " faces");
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                var f = mesh.Faces.GetFaceVertices(i);
                sw.Write("f");
                if (f.Length == 4)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        sw.Write(" ");
                        sw.Write((f[j] + 1).ToString(ci));
                    }
                }
                else
                {
                    for (int j = 0; j < 3; j++)
                    {
                        sw.Write(" ");
                        sw.Write((f[j] + 1).ToString(ci));
                    }
                }
                sw.WriteLine();
            }
            sw.WriteLine("# end of OBJ file");

            sw.Flush();
            sw.Close();
            sw.Dispose();
        }
        public static PlanktonMesh ReadObj(string outputPath)
        {
            PlanktonMesh mm = new PlanktonMesh();
            if (File.Exists(outputPath))
            {
                StreamReader SR = new StreamReader(outputPath);
                while (!SR.EndOfStream)
                {
                    string line = SR.ReadLine();//Read Line

                    //for instance
                    /*
                    f A1/A2/A3 B1/B2/B3 C1/C2/C3 D1/D2/D3
                    */
                    string[] tokens = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);//Divide every line as name and values
                    if (tokens.Length !=0)
                    {
                        if (tokens[0] == "v")
                        {
                            mm.Vertices.Add(double.Parse(tokens[1]), double.Parse(tokens[2]), double.Parse(tokens[3]));
                        }
                        else if (tokens[0] == "f")
                        {
                            int num = tokens.Length - 1;//the count of vertices of face
                            List<int> indices = new List<int>();
                            for (int i = 0; i < num; i++)
                            {
                                string[] vertexTokens = tokens[i + 1].Split("/".ToCharArray());
                                indices.Add(int.Parse(vertexTokens[0]));
                            }
                            if (num == 3)//Triangle
                            {
                                mm.Faces.AddFace(int.Parse(indices[0].ToString()) - 1, int.Parse(indices[1].ToString()) - 1, int.Parse(indices[2].ToString()) - 1);
                            }
                            else if (num == 4)//Quadangle
                            {
                                mm.Faces.AddFace(int.Parse(indices[0].ToString()) - 1, int.Parse(indices[1].ToString()) - 1, int.Parse(indices[2].ToString()) - 1, int.Parse(indices[3].ToString()) - 1);
                            }
                        }
                    }

                }
                SR.Close();
                SR.Dispose();
            }

            return mm;
        }
    }
}
