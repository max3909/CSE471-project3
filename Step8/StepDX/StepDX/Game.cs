using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace StepDX
{
    public partial class Game : Form
    {
        /// <summary>
        /// The DirectX device we will draw on
        /// </summary>
        private Device device = null;

        /// <summary>
        /// Height of our playing area (meters)
        /// </summary>
        private float playingH = 4;

        /// <summary>
        /// Width of our playing area (meters)
        /// </summary>
        private float playingW = 32;

        /// <summary>
        /// Vertex buffer for our drawing
        /// </summary>
        private VertexBuffer vertices = null;

        /// <summary>
        /// The background image class
        /// </summary>
        private Background background = null;

        /// <summary>
        /// What the last time reading was
        /// </summary>
        private long lastTime;

        /// <summary>
        /// A stopwatch to use to keep track of time
        /// </summary>
        private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        //private Vector2 playerLoc = new Vector2(0.4f, 1);   // Where our player is
        //private Vector2 playerSpeed = new Vector2(0, 0);    // How fast we are moving
        //private Vector2 playerAccel = new Vector2(0, 0);
        //float playerMinX = 0.4f;                    // Minimum x allowed
        //float playerMaxX = 31.6f;                   // Maximum x allowed
        //float playerMinY = 1;

        bool jumped = false;

        /// <summary>
        /// Our player sprite
        /// </summary>
        GameSprite player = new GameSprite();

        /// <summary>
        /// All of the polygons that make up our world
        /// </summary>
        List<Polygon> world = new List<Polygon>();

        /// <summary>
        /// The collision testing subsystem
        /// </summary>
        Collision collision = new Collision();

        public Game()
        {
            InitializeComponent();
            if (!InitializeDirect3D())
                return;
            vertices = new VertexBuffer(typeof(CustomVertex.PositionColored), // Type of vertex
                                       4,      // How many
                                       device, // What device
                                       0,      // No special usage
                                       CustomVertex.PositionColored.Format,
                                       Pool.Managed);
            background = new Background(device, playingW, playingH);
            // Determine the last time
            stopwatch.Start();
            lastTime = stopwatch.ElapsedMilliseconds;
            AddObstacle(0, playingW, 0.9f, 1, Color.CornflowerBlue);
            AddObstacle(2, 3, 1.7f, 1.9f, Color.Crimson);
            AddObstacle(4, 4.2f, 1, 2.1f, Color.Coral);
            AddObstacle(5, 6, 2.2f, 2.4f, Color.BurlyWood);
            AddObstacle(5.5f, 6.5f, 3.2f, 3.4f, Color.PeachPuff);
            AddObstacle(6.5f, 7.5f, 2.5f, 2.7f, Color.Chocolate);
            AddPlatform(3.2f, 3.9f, 1.8f, 2, Color.CornflowerBlue);
            Texture texture = TextureLoader.FromFile(device, "../../../stone08.bmp");
            AddTexture(texture, 1.2f, 1.9f, 3.3f, 3.5f, Color.Transparent);
            Texture spritetexture = TextureLoader.FromFile(device, "../../../guy8.bmp");
            player.Tex = spritetexture;
            player.AddVertex(new Vector2(-0.2f, 0));
            player.AddTex(new Vector2(0, 1));
            player.AddVertex(new Vector2(-0.2f, 1));
            player.AddTex(new Vector2(0, 0));
            player.AddVertex(new Vector2(0.2f, 1));
            player.AddTex(new Vector2(0.125f, 0));
            player.AddVertex(new Vector2(0.2f, 0));
            player.AddTex(new Vector2(0.125f, 1));
            player.Color = Color.Transparent;

            player.Transparent = true;

            player.P = new Vector2(0.5f, 1);
            
        }

        /// <summary>
        /// Initialize the Direct3D device for rendering
        /// </summary>
        /// <returns>true if successful</returns>
        private bool InitializeDirect3D()
        {
            try
            {
                // Now let's setup our D3D stuff
                PresentParameters presentParams = new PresentParameters();
                presentParams.Windowed = true;
                presentParams.SwapEffect = SwapEffect.Discard;

                device = new Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, presentParams);
            }
            catch (DirectXException)
            {
                return false;
            }

            return true;
        }

        public void Render()
        {
            if (device == null)
                return;

            device.Clear(ClearFlags.Target, System.Drawing.Color.Blue, 1.0f, 0);

            int wid = Width;                            // Width of our display window
            int hit = Height;                           // Height of our display window.
            float aspect = (float)wid / (float)hit;     // What is the aspect ratio?

            device.RenderState.ZBufferEnable = false;   // We'll not use this feature
            device.RenderState.Lighting = false;        // Or this one...
            device.RenderState.CullMode = Cull.None;    // Or this one...

            float widP = playingH * aspect;         // Total width of window
             float winCenter = player.P.X;
            if (winCenter - widP / 2 < 0)
                winCenter = widP / 2;
            else if (winCenter + widP / 2 > playingW)
                winCenter = playingW - widP / 2;

            device.Transform.Projection = Matrix.OrthoOffCenterLH(winCenter - widP/2, 
                                                                  winCenter + widP/2, 
                                                                  0, playingH, 0, 1);

            //Begin the scene
            device.BeginScene();

            // Render the background
            background.Render();
           

            foreach (Polygon p in world)
            {
                p.Render(device);
            }

            player.Render(device);

            //End the scene
            device.EndScene();
            device.Present();
        }

        protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close(); // Esc was pressed
            else if (e.KeyCode == Keys.Right)
            {
                Vector2 v = player.V;
                v.X = 1.5f;
                player.V = v;
            }
            else if (e.KeyCode == Keys.Left)
            {
                Vector2 v = player.V;
                v.X = -1.5f;
                player.V = v;
            }
            else if (e.KeyCode == Keys.Space)
            {
                if (!jumped)
                {
                    jumped = true;
                    player.standing = false;
                    Vector2 v = player.V;
                    v.Y = 7;
                    player.V = v;
                    player.A = new Vector2(0, -9.8f);
                }
               
            }

        }

        protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Left)
            {
                Vector2 v = player.V;
                v.X = 0;
                player.V = v;
            }
        }
       

        /// <summary>
        /// Advance the game in time
        /// </summary>
        public void Advance()
        {
            // How much time change has there been?
            long time = stopwatch.ElapsedMilliseconds;
            float delta = (time - lastTime) * 0.001f;       // Delta time in milliseconds
            lastTime = time;

            while (delta > 0)
            {

                float step = delta;
                if (step > 0.05f)
                    step = 0.05f;

                float maxspeed = Math.Max(Math.Abs(player.V.X), Math.Abs(player.V.Y));
                if (maxspeed > 0)
                {
                    step = (float)Math.Min(step, 0.05 / maxspeed);
                }

                player.Advance(step);

                foreach (Polygon p in world)
                    p.Advance(step);

                foreach (Polygon p in world)
                {
                    if (collision.Test(player, p))
                    {
                        float depth = collision.P1inP2 ?
                                  collision.Depth : -collision.Depth;
                        player.P = player.P + collision.N * depth;
                        Vector2 v = player.V;
                        if (collision.N.X != 0)
                            v.X = 0;
                        if (collision.N.Y != 0)
                            v.Y = 0;
                        if (collision.N.Y > 0.0f)
                        {
                            jumped = false;
                            player.standing = true;
                        }
                        player.V = v;
                        player.Advance(0);
                    }
                }

                if (player.P.X < 0.0f)
                {
                    Vector2 p = player.P;
                    p.X = 0.0f;
                    player.P = p;
                }
                else if (player.P.X > playingW)
                {
                    Vector2 p = player.P;
                    p.X = playingW;
                    player.P = p;
                }

                delta -= step;
            }



        }

        public void AddObstacle(float left, float right, float bottom, float top, Color color)
        {
            Polygon thing = new Polygon();
            thing.AddVertex(new Vector2(left, top));
            thing.AddVertex(new Vector2(right, top));
            thing.AddVertex(new Vector2(right, bottom));
            thing.AddVertex(new Vector2(left, bottom));
            thing.Color = color;
            world.Add(thing);
        }

        public void AddPlatform(float left, float right, float bottom, float top, Color color)
        {
            Platform thing = new Platform();
            thing.AddVertex(new Vector2(left, top));
            thing.AddVertex(new Vector2(right, top));
            thing.AddVertex(new Vector2(right, bottom));
            thing.AddVertex(new Vector2(left, bottom));
            thing.Color = color;
            world.Add(thing);
        }

        public void AddTexture(Texture tex, float left, float right, float bottom, float top, Color color)
        {
            PolygonTextured pt = new PolygonTextured();
            pt.Tex = tex;
            pt.AddVertex(new Vector2(left, top));
            pt.AddTex(new Vector2(0, 1));
            pt.AddVertex(new Vector2(right, top));
            pt.AddTex(new Vector2(0, 0));
            pt.AddVertex(new Vector2(right, bottom));
            pt.AddTex(new Vector2(1, 0));
            pt.AddVertex(new Vector2(left, bottom));
            pt.AddTex(new Vector2(1, 1));
            pt.Color = color;
            world.Add(pt);
        }

    }
}
