using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace SU_57_WFA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    // Flight Simulator


    public partial class Form1 : Form
    {

        //Global Variables

        Flight_Dynamics FD = new Flight_Dynamics();
        Math_Library ML = new Math_Library();
        Aircraft_Dimensions AD = new Aircraft_Dimensions();
        Aircraft_Dimensions AD1 = new Aircraft_Dimensions();
        double[,] v = new double[12, 1];
        double[,] u = new double[4, 1];
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        //Simulation Variables
        double t = 0;
        string sim = "on";

        public double[,] GetFromFile(string[] args)
        {
            double[,] vert;
            using (var reader = new StreamReader(@"C:\Users\AERO UAV\Desktop\TU160.txt"))
            {
                List<double> listA = new List<double>();
                List<double> listB = new List<double>();
                List<double> listC = new List<double>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine(); 
                    var values = line.Split(',');

                    listA.Add(double.Parse(values[0]));
                    listB.Add(double.Parse(values[1]));
                    listC.Add(double.Parse(values[2]));

                }

                double[] A = listA.ToArray();
                double[] B = listB.ToArray();
                double[] C = listC.ToArray();

                vert = new double[A.GetLength(0), 3];

                for (int i = 0; i < A.GetLength(0); i++)
                {
                    vert[i, 0] = (double)A[i];
                    vert[i, 1] = (double)B[i];
                    vert[i, 2] = (double)C[i];
                }

            }

            return vert;

        }

        public Form1()
        {
            InitializeComponent();
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Start With Initial Condition
            //Get Initial Conditions
            FD.IC(v);

            //PD Gains
            double p = -2;
            double d = -2;
            double dist = 0;
            double distp = 0;
            double dg_p = 0;
            double d_dt = 0;
            double g_dt = 0;
            double gamma = 0;
            double offset = Math.PI / 2;
            double vel_m = 0;
            double spd_c = 5;
            string[] args = new string[0];
            double[,] Vertices = GetFromFile(args);

            //Vertices = AD.Shift(Vertices);

            int init = 0;

            while (true)
            {
                double gain = 10;
                //Get Inputs
                double u_t = (double)GetAsyncKeyState('P');
                u_t = -(double)GetAsyncKeyState('L');

                double u_phi = -(double)GetAsyncKeyState('A');
                u_phi = (double)GetAsyncKeyState('D');

                double u_theta = -(double)GetAsyncKeyState('S');
                u_theta = (double)GetAsyncKeyState('W');

                double u_gamma = -(double)GetAsyncKeyState('N');
                u_gamma = (double)GetAsyncKeyState('M');

                if (Math.Abs(u_t) > 1)
                    u_t = Math.Sign(u_t) * 1;
                if (Math.Abs(u_phi) > 1)
                    u_phi = Math.Sign(u_phi) * 1;
                if (Math.Abs(u_theta) > 1)
                    u_theta = Math.Sign(u_theta) * 1;
                if (Math.Abs(u_gamma) > 1)
                    u_gamma = Math.Sign(u_gamma) * 1;

                


                if (checkBox1.Checked)
                {

                    distp = dist;
                    dist = ML.Euclidian_Norm(-double.Parse(textBox1.Text.ToString()) + v[0,0], -double.Parse(textBox2.Text.ToString()) + v[2, 0], -double.Parse(textBox3.Text.ToString()) + v[4, 0]);
                    vel_m = ML.Euclidian_Norm(v[1,0],v[3,0],v[5,0]);
                    d_dt = (dist - distp) / .1;

                    u_t = -.01*p * dist + .001*d*d_dt;
                    //u_t = 10*gamma;
                    //u_t = 0;

                    dg_p = gamma;
                   

                    if(v[6,0] > Math.PI )
                    {
                        v[6, 0] = v[6, 0] - 2*Math.PI;
                    }

                    if(v[6,0] <= -Math.PI)
                    {
                        v[6, 0] = v[6, 0] + 2*Math.PI;
                    }

                    gamma = (v[6, 0] - Math.Atan2(double.Parse(textBox2.Text.ToString()) - v[2, 0], double.Parse(textBox1.Text.ToString()) - v[0, 0])) + Math.PI / 2;


                    textBox3.Text = gamma.ToString();
                    textBox3.Refresh();

                    u_gamma = .1*p*gamma + .1*d*(gamma - dg_p)/.1;
                    u_t = p*(vel_m - spd_c);

                    //Convergence Criteria 2-D

                    if (Math.Abs(v[0,0]-double.Parse(textBox1.Text)) <= 3 & Math.Abs(v[2,0]-double.Parse(textBox2.Text)) <= 3)
                    {
                        u_t = -p*(vel_m - spd_c);
                    }

                    u[0, 0] = u_t;
                    u[1, 0] = gain*u_gamma;
                    u[2, 0] = gain * 0;
                    u[3, 0] = 0;
                }
                else
                {
                    u[0, 0] = gain * u_t;
                    u[1, 0] = gain * u_phi;
                    u[2, 0] = gain * u_theta;
                    u[3, 0] = gain * u_gamma;
                }

                Console.WriteLine(u[0, 0].ToString() + " " + u[1, 0].ToString() + " " + u[2, 0].ToString() + " " + u[3, 0].ToString());

                //Get
                v = FD.FE(FD.Derivatives(v, u), v);

                Console.WriteLine(v[0, 0].ToString() + " " + v[1, 0].ToString() + " " + u[2, 0].ToString() + " " + v[3, 0].ToString() + " " + v[4, 0].ToString() + " " + v[5, 0].ToString() + " " + v[6, 0].ToString() + " " + v[7, 0].ToString() + v[8, 0].ToString() + " " + v[9, 0].ToString() + " " + v[10, 0].ToString() + " " + v[11, 0].ToString());

                double[,] AV_s = new double[9,1];
                double[,] AV1 = new double[9,1];

                if (init == 0)
                {
                    Vertices = AD.Shift(Vertices);
                    Vertices = AD1.Shift(Vertices);
                    init = 1;
                }

                //AV_s = AD.GetVertices_ff(v,Vertices);
                //AV1 = AD1.GetVertices_RPY_ff(v,Vertices);

                AV_s = AD.GetVertices_ff(v,Vertices);
                AV1 = AD1.GetVertices_RPY_ff(v,Vertices);

                for (int i = 0; i < AV_s.GetLength(0); i++)
                {
                    chart1.Series[0].Points.AddXY(AV_s[i, 0], AV_s[i, 1]);
                    chart2.Series[0].Points.AddXY(AV1[i, 0], AV1[i, 1]);
                    chart3.Series[0].Points.AddXY(AV1[i, 1], AV1[i, 2]);
                    chart4.Series[0].Points.AddXY(AV1[i, 0], AV1[i, 2]);
                }

                chart1.ChartAreas[0].AxisX.Maximum = 250;
                chart1.ChartAreas[0].AxisX.Minimum = -250;
                chart1.ChartAreas[0].AxisY.Maximum = 250;
                chart1.ChartAreas[0].AxisY.Minimum = -250;

                chart2.ChartAreas[0].AxisX.Maximum = 10;
                chart2.ChartAreas[0].AxisX.Minimum = -10;
                chart2.ChartAreas[0].AxisY.Maximum = 10;
                chart2.ChartAreas[0].AxisY.Minimum = -10;

                chart3.ChartAreas[0].AxisX.Maximum = 10;
                chart3.ChartAreas[0].AxisX.Minimum = -10;
                chart3.ChartAreas[0].AxisY.Maximum = 10;
                chart3.ChartAreas[0].AxisY.Minimum = -10;

                chart4.ChartAreas[0].AxisX.Maximum = 10;
                chart4.ChartAreas[0].AxisX.Minimum = -10;
                chart4.ChartAreas[0].AxisY.Maximum = 10;
                chart4.ChartAreas[0].AxisY.Minimum = -10;

                chart1.Refresh();
                chart2.Refresh();
                chart3.Refresh();
                chart4.Refresh();
                System.Threading.Thread.Sleep(100);
                chart1.Series[0].Points.Clear();
                chart2.Series[0].Points.Clear();
                chart3.Series[0].Points.Clear();
                chart4.Series[0].Points.Clear();

            }
            //double u_gamma = -(double)GetAsyncKeyState('A');
            //u_gamma = (double)GetAsyncKeyState('D');


        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }


    public class Aircraft_Dimensions
    {
        double[,] Vertices = new double[9, 3];
        double[,] Center = new double[1, 3];
        Math_Library ML = new Math_Library();


        public double[,] GetVertices_del(double[,] v)
        {

            Center[0, 0] = 5;
            Center[0, 1] = 0;
            Center[0, 2] = 0;

            Vertices[0, 0] = 2 - Center[0, 0];
            Vertices[0, 1] = 0 - Center[0, 1];
            Vertices[0, 2] = 0 - Center[0, 2];

            Vertices[1, 0] = 0 - Center[0, 0];
            Vertices[1, 1] = 0 - Center[0, 1];
            Vertices[1, 2] = 2 - Center[0, 2];

            Vertices[2, 0] = 0 - Center[0, 0];
            Vertices[2, 1] = 0 - Center[0, 1];
            Vertices[2, 2] = 0 - Center[0, 2];

            Vertices[3, 0] = 0 - Center[0, 0];
            Vertices[3, 1] = 5 - Center[0, 1];
            Vertices[3, 2] = 0 - Center[0, 2];

            Vertices[4, 0] = 7 - Center[0, 0];
            Vertices[4, 1] = 1 - Center[0, 1];
            Vertices[4, 2] = 0 - Center[0, 2];

            Vertices[5, 0] = 10 - Center[0, 0];
            Vertices[5, 1] = 0 - Center[0, 1];
            Vertices[5, 2] = 0 - Center[0, 2];

            Vertices[6, 0] = 7 - Center[0, 0];
            Vertices[6, 1] = -1 - Center[0, 1];
            Vertices[6, 2] = 0 - Center[0, 2];

            Vertices[7, 0] = 0 - Center[0, 0];
            Vertices[7, 1] = -5 - Center[0, 1];
            Vertices[7, 2] = 0 - Center[0, 2];

            Vertices[8, 0] = 0 - Center[0, 0];
            Vertices[8, 1] = 0 - Center[0, 1];
            Vertices[8, 2] = 0 - Center[0, 2];

            for (int i = 0; i < Vertices.GetLength(0); i++)
            {

                double d_x;
                double d_y;
                double d_z;

                double d_x_hat;
                double d_y_hat;
                double d_z_hat;

                double d_x_hat_hat = Vertices[i, 0];
                double d_y_hat_hat = Vertices[i, 1];
                double d_z_hat_hat = Vertices[i, 2];

                double[,] holder = new double[2, 1];

                holder = ML.Rotation(d_y_hat_hat, d_z_hat_hat, -v[10, 0]);

                d_y_hat = holder[0, 0];
                d_z_hat = holder[1, 0];

                holder = ML.Rotation(d_x_hat_hat, d_z_hat, -v[8, 0]);

                d_x_hat = holder[0, 0];
                d_z = holder[1, 0];

                holder = ML.Rotation(d_x_hat, d_y_hat, -v[6, 0]);

                d_x = holder[0, 0];
                d_y = holder[1, 0];

                Vertices[i, 0] = d_x + v[0,0];
                Vertices[i, 1] = d_y + v[2,0];
                Vertices[i, 2] = d_z + v[4,0];

            }

            return Vertices;
        }

        public double[,] GetVertices_RPY_del(double[,] v)
        {

            Center[0, 0] = 5;
            Center[0, 1] = 0;
            Center[0, 2] = 0;

            Vertices[0, 0] = 2 - Center[0, 0];
            Vertices[0, 1] = 0 - Center[0, 1];
            Vertices[0, 2] = 0 - Center[0, 2];

            Vertices[1, 0] = 0 - Center[0, 0];
            Vertices[1, 1] = 0 - Center[0, 1];
            Vertices[1, 2] = 2 - Center[0, 2];

            Vertices[2, 0] = 0 - Center[0, 0];
            Vertices[2, 1] = 0 - Center[0, 1];
            Vertices[2, 2] = 0 - Center[0, 2];

            Vertices[3, 0] = 0 - Center[0, 0];
            Vertices[3, 1] = 5 - Center[0, 1];
            Vertices[3, 2] = 0 - Center[0, 2];

            Vertices[4, 0] = 7 - Center[0, 0];
            Vertices[4, 1] = 1 - Center[0, 1];
            Vertices[4, 2] = 0 - Center[0, 2];

            Vertices[5, 0] = 10 - Center[0, 0];
            Vertices[5, 1] = 0 - Center[0, 1];
            Vertices[5, 2] = 0 - Center[0, 2];

            Vertices[6, 0] = 7 - Center[0, 0];
            Vertices[6, 1] = -1 - Center[0, 1];
            Vertices[6, 2] = 0 - Center[0, 2];

            Vertices[7, 0] = 0 - Center[0, 0];
            Vertices[7, 1] = -5 - Center[0, 1];
            Vertices[7, 2] = 0 - Center[0, 2];

            Vertices[8, 0] = 0 - Center[0, 0];
            Vertices[8, 1] = 0 - Center[0, 1];
            Vertices[8, 2] = 0 - Center[0, 2];

            for (int i = 0; i < Vertices.GetLength(0); i++)
            {

                double d_x;
                double d_y;
                double d_z;

                double d_x_hat;
                double d_y_hat;
                double d_z_hat;

                double d_x_hat_hat = Vertices[i, 0];
                double d_y_hat_hat = Vertices[i, 1];
                double d_z_hat_hat = Vertices[i, 2];

                double[,] holder = new double[2, 1];

                holder = ML.Rotation(d_y_hat_hat, d_z_hat_hat, -v[10, 0]);

                d_y_hat = holder[0, 0];
                d_z_hat = holder[1, 0];

                holder = ML.Rotation(d_x_hat_hat, d_z_hat, -v[8, 0]);

                d_x_hat = holder[0, 0];
                d_z = holder[1, 0];

                holder = ML.Rotation(d_x_hat, d_y_hat, -v[6, 0]);

                d_x = holder[0, 0];
                d_y = holder[1, 0];

                Vertices[i, 0] = d_x;
                Vertices[i, 1] = d_y;
                Vertices[i, 2] = d_z;

            }

            return Vertices;
        }

        
        public double[,] Shift(double[,] Vertices)
        {
            Center[0, 0] = 0;
            Center[0, 1] = 0;
            Center[0, 2] = 0;

            for (int i = 0; i < Vertices.GetLength(0); i++)
            {
                Vertices[i, 0] = Vertices[i, 0] - Center[0, 0];
                Vertices[i, 0] = Vertices[i, 0] - Center[0, 1];
                Vertices[i, 0] = Vertices[i, 0] - Center[0, 2];
            }

            return Vertices;
        }

        public double[,] GetVertices_ff(double[,] v, double[,] Vertices)
        {

            for (int i = 0; i < Vertices.GetLength(0); i++)
            {

                double d_x;
                double d_y;
                double d_z;

                double d_x_hat;
                double d_y_hat;
                double d_z_hat;

                double d_x_hat_hat = Vertices[i, 0];
                double d_y_hat_hat = Vertices[i, 1];
                double d_z_hat_hat = Vertices[i, 2];

                double[,] holder = new double[2, 1];

                holder = ML.Rotation(d_y_hat_hat, d_z_hat_hat, -v[10, 0]);

                d_y_hat = holder[0, 0];
                d_z_hat = holder[1, 0];

                holder = ML.Rotation(d_x_hat_hat, d_z_hat, -v[8, 0]);

                d_x_hat = holder[0, 0];
                d_z = holder[1, 0];

                holder = ML.Rotation(d_x_hat, d_y_hat, -v[6, 0]);

                d_x = holder[0, 0];
                d_y = holder[1, 0];

                Vertices[i, 0] = d_x + v[0, 0];
                Vertices[i, 1] = d_y + v[2, 0];
                Vertices[i, 2] = d_z + v[4, 0];

            }

            return Vertices;
        }

        public double[,] GetVertices_RPY_ff(double[,] v, double[,] Vertices)
        {

            for (int i = 0; i < Vertices.GetLength(0); i++)
            {

                double d_x;
                double d_y;
                double d_z;

                double d_x_hat;
                double d_y_hat;
                double d_z_hat;

                double d_x_hat_hat = Vertices[i, 0];
                double d_y_hat_hat = Vertices[i, 1];
                double d_z_hat_hat = Vertices[i, 2];

                double[,] holder = new double[2, 1];

                holder = ML.Rotation(d_y_hat_hat, d_z_hat_hat, -v[10, 0]);

                d_y_hat = holder[0, 0];
                d_z_hat = holder[1, 0];

                holder = ML.Rotation(d_x_hat_hat, d_z_hat, -v[8, 0]);

                d_x_hat = holder[0, 0];
                d_z = holder[1, 0];

                holder = ML.Rotation(d_x_hat, d_y_hat, -v[6, 0]);

                d_x = holder[0, 0];
                d_y = holder[1, 0];

                Vertices[i, 0] = d_x;
                Vertices[i, 1] = d_y;
                Vertices[i, 2] = d_z;

            }

            return Vertices;
        }

    }


    public class Flight_Dynamics
    {

        Math_Library ML = new Math_Library();

        double[,] Der = new double[12, 1];

        //State Vector 
        double[,] v = new double[12, 1];

        //Input Vector
        double[,] u = new double[12, 1];

        //Derivative Vector
        double[,] v_dot = new double[12, 1];

        //Time Step
        double dt = .1;

        //Drag Coefficients (Only In Direction of Travel)
        double B_t = 1;

        //Mass (kg)
        double m = 1;

        //Drag Coefficient;
        double B = 1;

        public double[,] IC(double[,] v)
        {
            //Inertial Frame
            //x, y, z, x_dot, y_dot, z_dot, roll, roll_dot, pitch, pitch_dot, yaw, yaw_dot

            v[0, 0] = 0;
            v[1, 0] = 0;
            v[2, 0] = 0;
            v[3, 0] = 0;
            v[4, 0] = 0;
            v[5, 0] = 0;
            v[6, 0] = 0;
            v[7, 0] = 0;
            v[8, 0] = 0;
            v[9, 0] = 0;
            v[10, 0] = 0;
            v[11, 0] = 0;

            return v;

        }

        //Derivative in Relative Frame 

        public double[,] Derivatives(double[,] v, double[,] u)
        {

            double vel_m = ML.Euclidian_Norm(v[1, 0], v[3, 0], v[5, 0]);
            double[,] Der = new double[8, 1];
            //Derivative

            if(u[0,0] == 100)
            {
                u[0, 0] = 100;
            }

            //x_hat_hat
            //Der[0, 0] = vel_m;
            Der[1, 0] = -B_t * vel_m / m + u[0, 0] / m;
            Der[0, 0] = vel_m;

            //roll_hat_hat
            Der[2, 0] = v[7, 0];
            Der[3, 0] = -B * v[7, 0] / m + u[1, 0] / m;

            //pitch_hat_hat
            Der[4, 0] = v[9, 0];
            Der[5, 0] = -B * v[9, 0] / m + u[2, 0] / m;

            //yaw_hat_hat
            Der[6, 0] = v[11, 0];
            Der[7, 0] = -B * v[11, 0] / m + u[3, 0] / m;

            return Der;

        }

        public double[,] FE(double[,] Der, double[,] v)
        {
            double vel_m = ML.Euclidian_Norm(v[1, 0], v[3, 0], v[5, 0]);

            double d_x;
            double d_y;
            double d_z;

            double d_x_hat;
            double d_y_hat;
            double d_z_hat;

            double d_x_hat_hat = (Der[0,0]*dt+dt*dt*Der[1,0]/2)*Math.Cos(v[6,0])*Math.Cos(v[10,0]);
            double d_y_hat_hat = (Der[0,0]*dt+dt*dt*Der[1,0]/2)*Math.Cos(v[6,0])*Math.Sin(v[8,0]);
            double d_z_hat_hat = (Der[0,0]*dt+dt*dt*Der[1,0]/2)*Math.Sin(v[6,0]);

            double[,] holder = new double[2, 1];

            holder = ML.Rotation(d_y_hat_hat, d_z_hat_hat, -v[10, 0]);

            d_y_hat = holder[0, 0];
            d_z_hat = holder[1, 0];

            holder = ML.Rotation(d_x_hat_hat, d_z_hat, -v[8, 0]);

            d_x_hat = holder[0, 0];
            d_z = holder[1, 0];

            holder = ML.Rotation(d_x_hat, d_y_hat, -v[6, 0]);

            d_x = holder[0, 0];
            d_y = holder[1, 0];

            v[0, 0] = v[0, 0] + d_x;
            v[1, 0] = d_x / dt;
            v[2, 0] = v[2, 0] + d_y;
            v[3, 0] = d_y / dt;
            v[4, 0] = v[4, 0] + d_z;
            v[5, 0] = d_z / dt;

            v[6, 0] = v[6, 0] + dt * Der[2, 0];
            v[7, 0] = v[7, 0] + dt * Der[3, 0];
            v[8, 0] = v[8, 0] + dt * Der[4, 0];
            v[9, 0] = v[9, 0] + dt * Der[5, 0];
            v[10, 0] = v[10, 0] + dt * Der[6, 0];
            v[11, 0] = v[11, 0] + dt * Der[7, 0];

            return v;

        }

    }

    public class Math_Library
    {

        public double[,] Rotation(double xp, double yp, double theta)
        {
            double x;
            double y;
            double[,] v_rotate = new double[2, 1];

            x = xp * Math.Cos(theta) - yp * Math.Sin(theta);
            y = xp * Math.Sin(theta) + yp * Math.Cos(theta);

            v_rotate[0, 0] = x;
            v_rotate[1, 0] = y;

            return v_rotate;
        }

        public double Euclidian_Norm(double vx, double vy, double vz)
        {

            double s = vx * vx + vy * vy + vz * vz;
            s = Math.Sqrt(s);

            return s;

        }

    }

}