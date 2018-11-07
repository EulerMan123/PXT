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

namespace PXT
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            //get_velocity_profile_NRT(double.Parse(label1.Text), int.Parse(label2.Text));
            string xpos = textBox1.Text;
            string seg = textBox2.Text;
            string rt = textBox3.Text;
            string t_total = textBox4.Text;

            chart1.Series["Series1"].Points.Clear();
            chart2.Series["Series1"].Points.Clear();

            //get_velocity_profile_DRT(Double.Parse(xpos), int.Parse(seg), Double.Parse(rt));

            get_velocity_profile_NRT_XST(Double.Parse(xpos), int.Parse(seg), Double.Parse(t_total));

        }

        public double[] get_velocity_profile_NRT_XST(double x_target, int seg, double t_total)
        {

            double del_L1 = 0;
            double del_L2 = 0;
            double del_l3 = 0;

            double amax = 1;
            double vmax = 6;
            double J0 = 1;
            //double J2 = 0;
            double v12;

            double t1 = 0;
            double t2 = 0;
            double t3 = 0;
            double t4 = 0;

            double xv = 10;


            double dt = (double)(1.0 / 24.0);

            double[] v = new double[1];
            double[] time = new double[1];

            double[] V = new double[1];
            double[] TIME = new double[1];
            double[] POS = new double[1];

            double desired_rise_time = 10;

            int i = 0;
            int current_index;
            int current_index_2;

            double[] v_tail;
            double[] t_reverse;
            double[] pos;
            double current_time;
            double x_total = 0;
            double x_tail = 0;

            bool go = true;

            //int seg = 4;
            //seg = 6;
            //seg = 5;
            //seg = 6;

            int k = 0;

            while (go) {

                i = 0;
                
                v = new double[1];
                time = new double[1];

                //Equation 1
                t1 = (-amax + Math.Sqrt(amax * amax + 4 * (J0 / 6.0) * vmax)) / J0;
                //Equation 2
                t2 = t1 + vmax / (3 * (amax + J0 * t1));

                if (seg == 5 || seg == 4)
                {
                    t2 = t1;
                }

                //Equation 3
                t3 = t2 + (amax + J0 * t1) / 2;
                //Equation 4

                if (k < 1) {
                    t4 = t3 + (t_total - 2 * (t1 + t2 + t3)) / 2;
                }

                k = k + 1;

                if (seg == 4 || seg == 6)
                {
                    t4 = t3;
                }

                for (double t = 0; t < t1; t = t + dt)
                {
                    v[i] = (J0 / 2) * (t) * (t) + amax * t;
                    time[i] = t;
                    v = DynamicArray(v);
                    time = DynamicArray(time);
                    i = i + 1;
                }

                for (double t = t1; t < t2; t = t + dt)
                {
                    v[i] = (J0 * t1 + amax) * (t - t1) + (1.0 / 3.0) * vmax;
                    time[i] = t;
                    v = DynamicArray(v);
                    time = DynamicArray(time);
                    i = i + 1;
                }

                for (double t = t2; t < t3; t = t + dt)
                {

                    if (seg == 7 || seg == 6)
                    {
                        v[i] = (-(amax + J0 * t1) / 2) * (t - t2) * (t - t2) + (amax + J0 * t1) * (t - t2) + (2.0 / 3.0) * vmax;
                    }
                    else if (seg == 5)
                    {
                        v[i] = (-(amax + J0 * t1) / 2) * (t - t2) * (t - t2) + (amax + J0 * t1) * (t - t2) + (1.0 / 3.0) * vmax;
                    }
                    else
                    {
                        v[i] = (-(amax + J0 * t1) / 2) * (t - t2) * (t - t2) + (amax + J0 * t1) * (t - t2) + (1.0 / 3.0) * vmax;
                    }

                    time[i] = t;
                    v = DynamicArray(v);
                    time = DynamicArray(time);
                    i = i + 1;

                }

                v_tail = new double[v.Length];
                t_reverse = new double[v.Length];

                v_tail = Reverse(v);

                current_index = i - 1;

                for (double t = t3; t < t4; t = t + dt)
                {
                    v[i] = v[current_index];
                    time[i] = t;
                    v = DynamicArray(v);
                    time = DynamicArray(time);
                    i = i + 1;
                }

                current_time = t4;
                current_index_2 = v.Length - 1;

                double[] v2 = Reverse(v);
                double[] time2 = Reverse(time);

                V[0] = v[0];
                TIME[0] = time[0];
                POS = new double[1];

                double maxV = 0;

                bool swi = false;
                int I = 0;
                int N = 0;

                for (i = 1; i < v.Length + v2.Length; i++)
                {
                    if (i < v.Length)
                    {
                        V[i-1] = v[i];
                        TIME[i-1] = i*dt;
                    }
                    else
                    {
                        V[i-2] = v2[i-v.Length];
                        TIME[i-2] = i*dt;
                    }

                    V = DynamicArray(V);
                    TIME = DynamicArray(TIME);

                }

                double gain = -.1;

                if(Math.Abs(x_total - x_target) < 1)
                {
                    go = false;
                }
                else
                {
                    vmax = vmax + gain * (x_total - x_target);
                }

                x_total = 0;

                for (int j = 0; j < V.Length - 1; j++)
                {
                    x_total = (V[j] + V[j + 1]) / 2 * dt + x_total;
                    POS[j] = x_total;
                    POS = DynamicArray(POS);
                }

            }

            for (int l = 0; l < V.Length - 5; l++)
            {
                chart1.Series["Series1"].Points.AddXY(TIME[l], V[l]);
                chart2.Series["Series1"].Points.AddXY(TIME[l], POS[l]);
            }

            return V;

        }

        public double[] get_velocity_profile_NRT(double x_target, int seg) {

            double del_L1 = 0;
            double del_L2 = 0;
            double del_l3 = 0;

            double amax = 1;
            double vmax = 6;
            double J0 = 1;
            //double J2 = 0;
            double v12;

            double t1 = 0;
            double t2 = 0;
            double t3 = 0;
            double t4 = 0;

            double xv = 10;


            double dt = (double)(1.0/24.0);

            double[] v = new double[1];
            double[] time = new double[1];

            double desired_rise_time = 10;

            int i = 0;
            int current_index;
            int current_index_2;

            double[] v_tail;
            double[] t_reverse;
            double[] pos;
            double current_time;
            double x_total = 0;
            double x_tail = 0;

            //int seg = 4;
            //seg = 6;
            //seg = 5;
            //seg = 6;

            for (int k = 0; k < 10; k++)
            {

                i = 0;

                v = new double[1];
                time = new double[1];

                //Equation 1
                t1 = (-amax + Math.Sqrt(amax * amax + 4 * (J0 / 6.0) * vmax)) / J0;
                //Equation 2
                t2 = t1 + vmax / (3 * (amax + J0 * t1));
                
                if(seg == 5 || seg == 4)
                {
                    t2 = t1;
                }

                //Equation 3
                t3 = t2 + (amax + J0 * t1) / 2;
                //Equation 4
                t4 = t3 + xv / vmax;

                if(seg == 4 || seg == 6)
                {
                    t4 = t3;
                }

                for (double t = 0; t < t1; t = t + dt)
                {
                    v[i] = (J0 / 2) * (t) * (t) + amax * t;
                    time[i] = t;
                    v = DynamicArray(v);
                    time = DynamicArray(time);
                    i = i + 1;
                }

                for (double t = t1; t < t2; t = t + dt)
                {
                    v[i] = (J0 * t1 + amax) * (t - t1) + (1.0 / 3.0) * vmax;
                    time[i] = t;
                    v = DynamicArray(v);
                    time = DynamicArray(time);
                    i = i + 1;
                }

                for (double t = t2; t < t3; t = t + dt)
                {

                    if(seg == 7 || seg == 6)
                    {
                        v[i] = (-(amax + J0 * t1) / 2) * (t - t2) * (t - t2) + (amax + J0 * t1) * (t - t2) + (2.0 / 3.0) * vmax;
                    }
                    else if(seg == 5)
                    {
                        v[i] = (-(amax + J0 * t1) / 2) * (t - t2) * (t - t2) + (amax + J0 * t1) * (t - t2) + (1.0 / 3.0) * vmax;
                    }
                    else
                    {
                        v[i] = (-(amax + J0 * t1) / 2) * (t - t2) * (t - t2) + (amax + J0 * t1) * (t - t2) + (1.0 / 3.0) * vmax;
                    }

                    time[i] = t;
                    v = DynamicArray(v);
                    time = DynamicArray(time);
                    i = i + 1;

                }

                v_tail = new double[v.Length];
                t_reverse = new double[v.Length];

                v_tail = Reverse(v);

                current_index = i - 1;

                for (double t = t3; t < t4; t = t + dt)
                {
                    v[i] = v[current_index];
                    time[i] = t;
                    v = DynamicArray(v);
                    time = DynamicArray(time);
                    i = i + 1;
                }

                current_time = t4;
                current_index_2 = v.Length - 1;

                for (double t = 0; t < t3 - dt; t = t + dt)
                {
                    v[i] = v_tail[i - current_index_2];
                    time[i] = t + current_time;
                    v = DynamicArray(v);
                    time = DynamicArray(time);
                    i = i + 1;
                }
                
                pos = new double[v.Length];

                x_total = 0;
                x_tail = 0;

                for (int j = 0; j < v.Length-1; j++)
                {
                    x_total = (v[j]+v[j+1])/2 * dt + x_total;
                    pos[j] = x_total;
                }

                x_tail = (x_total - xv);
                xv = x_target - x_tail; 

                /*

                if (k == 9) {

                    for (int l = 0; l < v.Length-5; l++)
                    {
                        chart1.Series["Series1"].Points.AddXY(time[l], v[l]);
                        chart2.Series["Series1"].Points.AddXY(time[l], pos[l]);
                    }

                    using (StreamWriter writer = new StreamWriter(Directory.GetCurrentDirectory().ToString()))
                    {

                        writer.Write("amax = " + amax.ToString() + ", vmax = " + vmax.ToString() + ", x_actual = " + (x_target-10).ToString() + ", x_total = " + x_total.ToString() + ", Jerk_initial = " + J0.ToString() + " Ramp Time = " + (t1+t2+t3).ToString() + " \n");
                        
                        for (i = 0; i < v.Length; i++)
                        {
                            writer.Write(time[i].ToString() + "," + pos[i].ToString() + "," + v[i].ToString() + " \n");
                        }
                    }
                }

                */

            }


            return v;

        }

        public double[] get_velocity_profile_DRT(double x_target, int seg, double RT)
        {

            double del_L1 = 0;
            double del_L2 = 0;
            double del_l3 = 0;

            double amax_n = 1;
            double vmax_n = 6;
            double amax = amax_n;
            double vmax = vmax_n;
            double a_p = .5 * amax;
            double v_p = .5 * vmax;
            double J0 = 1;
            //double J2 = 0;
            double v12;

            double t1 = 0;
            double t2 = 0;
            double t3 = 0;
            double t4 = 0;

            double xv = 10;
            double error = .01;

            double dt = (double)(1.0 / 24.0);

            double[] v = new double[1];
            double[] time = new double[1];

            double error_p = 1;

            double rise_time = 0;
            double gain = .01;
            double current_time;
            //int seg = 4;
            //seg = 6;
            //seg = 5;
            //seg = 6;

            double[] v_tail;
            double[] t_reverse;

            double x_total;
            double x_tail;
            double[] pos;

            int current_index;
            int current_index_2;
            int i;

            bool RT_s = false;

            while (!RT_s) {
          
                for (int k = 0; k < 10; k++)
                {

                    v = new double[1];
                    time = new double[1];

                    i = 0;

                    //Equat ion 1
                    t1 = (-amax + Math.Sqrt(amax * amax + 4 * (J0 / 6.0) * vmax)) / J0;
                    //Equation 2
                    t2 = t1 + vmax / (3 * (amax + J0 * t1));

                    if (seg == 5 || seg == 4)
                    {
                        t2 = t1;
                    }

                    //Equation 3
                    t3 = t2 + (amax + J0 * t1) / 2;
                    //Equation 4

                    if (RT_s)
                    {
                        t4 = t3 + xv / vmax;
                    }
                    else
                    {
                        t4 = 0;
                    }


                    //Desired Rise Time
                    rise_time = t1 + t2 + t3;

                    if (a_p > amax_n)
                    {
                        a_p = amax_n;
                        amax = amax_n;
                    }
                    else
                    {
                        a_p = a_p + gain * (rise_time - RT);
                        amax = a_p;
                        amax = amax_n;
                    }

                    error_p = v_p;

                    if (v_p > vmax_n)
                    {
                        v_p = vmax;
                        vmax = vmax_n;
                    }
                    else
                    {
                        v_p = v_p + gain * (rise_time - RT);
                        vmax = v_p;
                        
                    }

                   
                    if(Math.Abs(v_p - error_p) < error || v_p == vmax_n)
                    {
                        RT_s = true;
                    }

                    if (seg == 4 || seg == 6)
                    {
                        t4 = t3;
                    }

                    for (double t = 0; t < t1; t = t + dt)
                    {
                        v[i] = (J0 / 2) * (t) * (t) + amax * t;
                        time[i] = t;
                        v = DynamicArray(v);
                        time = DynamicArray(time);
                        i = i + 1;
                    }

                    for (double t = t1; t < t2; t = t + dt)
                    {
                        v[i] = (J0 * t1 + amax) * (t - t1) + (1.0 / 3.0) * vmax;
                        time[i] = t;
                        v = DynamicArray(v);
                        time = DynamicArray(time);
                        i = i + 1;
                    }

                    for (double t = t2; t < t3; t = t + dt)
                    {

                        if (seg == 7 || seg == 6)
                        {
                            v[i] = (-(amax + J0 * t1) / 2) * (t - t2) * (t - t2) + (amax + J0 * t1) * (t - t2) + (2.0 / 3.0) * vmax;
                        }
                        else if (seg == 5)
                        {
                            v[i] = (-(amax + J0 * t1) / 2) * (t - t2) * (t - t2) + (amax + J0 * t1) * (t - t2) + (1.0 / 3.0) * vmax;
                        }
                        else
                        {
                            v[i] = (-(amax + J0 * t1) / 2) * (t - t2) * (t - t2) + (amax + J0 * t1) * (t - t2) + (1.0 / 3.0) * vmax;
                        }

                        time[i] = t;
                        v = DynamicArray(v);
                        time = DynamicArray(time);
                        i = i + 1;
                    }

                    v_tail = new double[v.Length];
                    t_reverse = new double[v.Length];

                    v_tail = Reverse(v);

                    current_index = i - 1;

                    for (double t = t3; t < t4; t = t + dt)
                    {
                        v[i] = v[current_index];
                        time[i] = t;
                        v = DynamicArray(v);
                        time = DynamicArray(time);
                        i = i + 1;
                    }

                    current_time = t4;
                    current_index_2 = v.Length - 1;

                    for (double t = 0; t < t3 - dt; t = t + dt)
                    {
                        v[i] = v_tail[i - current_index_2];
                        time[i] = t + current_time;
                        v = DynamicArray(v);
                        time = DynamicArray(time);
                        i = i + 1;
                    }

                    x_total = 0;
                    x_tail = 0;
                    pos = new double[v.Length];

                    for (int j = 0; j < v.Length - 1; j++)
                    {
                        x_total = (v[j] + v[j + 1]) / 2 * dt + x_total;
                        pos[j] = x_total;
                    }

                    x_tail = (x_total - xv);
                    xv = x_target - x_tail;

                    if (k == 9)
                    {

                        for (int l = 0; l < v.Length - 5; l++)
                        {
                            chart1.Series["Series1"].Points.AddXY(time[l], v[l]);
                            chart2.Series["Series1"].Points.AddXY(time[l], pos[l]);
                        }


                        //string again = Directory.GetCurrentDirectory();

                        using (StreamWriter writer = new StreamWriter(Directory.GetCurrentDirectory() + "\\x_pos_t_pos.csv"))
                        {

                            writer.Write("amax = " + amax.ToString() + ", vmax = " + vmax.ToString() + ", x_actual = " + (x_target).ToString() + ", x_total = " + x_total.ToString() + ", Jerk_initial = " + J0.ToString() + " Ramp Time = " + (t1 + t2 + t3).ToString() + " \n");

                            for (i = 0; i < v.Length; i++)
                            {
                                writer.Write(time[i].ToString() + "," + pos[i].ToString() + "," + v[i].ToString() + " \n");
                            }
                        }
                    }

                }

            }
            return v;
        }

        public double[] DynamicArray(double[] x)
        {

            double[] new_x = new double[x.Length + 1];
            new_x[0] = x[0];

            for (int i = 1; i < x.Length; i++)
            {
                new_x[i] = x[i];
            }

            return new_x;

        }

        public double[] Reverse(double[] x)
        {

            double[] y = new double[x.Length-1];

            for (int i = 1; i < x.Length; i++)
            {
                y[i-1] = x[x.Length-i-1];
            }

            return y;

        }

    }
}
