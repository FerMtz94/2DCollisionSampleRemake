using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CollisionTesting
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        World world;
        Body body, trigger;
        RayTracer ray;

        string onCollisionMessage = "";

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content para cargar aquí el contenido del juego
            this.IsMouseVisible = true;

            Helper.font = Content.Load<SpriteFont>("font");
            Helper.spriteBatch = spriteBatch;
            Helper.graphicDevice = GraphicsDevice;

            // Inicializamos el sistema de colisiones y añadimos objetos a la escena:
            world = new World();

            world.bodies.Add(new Body(new Rectangle(0, 0, 80, 80), Color.Green, true, true));
            world.bodies.Add(new Body(new Rectangle(0, 80, 80, 80), Color.Green, true, true));
            world.bodies.Add(new Body(new Rectangle(0, 80 * 2, 80, 80), Color.Green, true, true));
            world.bodies.Add(new Body(new Rectangle(0, 80 * 3, 80, 80), Color.Green, true, true));

            world.bodies.Add(new Body(new Rectangle(80, 80 * 3, 80, 80), Color.Green, true, true));
            world.bodies.Add(new Body(new Rectangle(80 * 2, 80 * 3, 80, 80), Color.Green, true, true));
            world.bodies.Add(new Body(new Rectangle(80 * 3, 80 * 3, 80, 80), Color.Green, true, true));

            world.bodies.Add(new Body(new Rectangle(80 * 3, 80, 80, 80), Color.Green, true, true));
            world.bodies.Add(new Body(new Rectangle(80 * 9, 80, 80, 80), Color.Green, true, true));

            world.bodies.Add(new Body(new Rectangle(0, 80 * 5, 80, 80), Color.Green, true, true));

            world.bodies.Add(new Body(new Rectangle(80 * 4, 80, 80, 20), Color.Green, true, true));

            world.bodies.Add(new Body(new Rectangle(80 * 2, 80, 80, 10), Color.Green, true, true));

            // Creamos un cuerpo no fijo que manejaremos como si fuera el jugador:
            body = new Body(new Rectangle(80 * 2, 80 * 2, 80, 80), Color.Red, false, true);
            body.Debug = true;
            world.bodies.Add(body);
            body.WorldInstance = world;
            body.PreUpdate += this.PreUpdate;
            body.PostUpdate += this.PostUpdate;
            body.PostDraw += this.PostDraw;

            // Creamos un cuerpo no solido que hara las veces de trigger (un lanzador de eventos):
            trigger = new Body(new Rectangle(80 * 6, 80 * 2, 80 * 2, 80 * 2), new Color(255, 0, 255, 128), true, false);
            world.bodies.Add(trigger);
            trigger.OnCollision += this.triggerOnCollision;

            // Definimos un rayo para trazar trayectorias con el entorno:
            ray = new RayTracer(world);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            onCollisionMessage = "";

            KeyboardState keyb = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: agregue aquí su lógica de actualización

            // Definimos los controles para desplazar el cuerpo por la escena:
            int speed = 30;
            if (keyb.IsKeyDown(Keys.D))
                body.Rectangle.X += speed;
            if (keyb.IsKeyDown(Keys.A))
                body.Rectangle.X -= speed;
            if (keyb.IsKeyDown(Keys.S))
                body.Rectangle.Y += speed;
            if (keyb.IsKeyDown(Keys.W))
                body.Rectangle.Y -= speed;

            world.Update(); // Actualizamos el estado de la escena.

            // Configuramos y trazamos el rayo:
            ray.Source = body.Center;
            ray.SourceBody = body;
            ray.Direction = MathTools.GetAngle(body.Center, new Vector2(mouse.X, mouse.Y));
            ray.Radius = 800;
            ray.Trace();

            base.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// Se llama cuando el juego debe realizar dibujos por sí mismo.
        /// </summary>
        /// <param name="gameTime">Proporciona una instantánea de los valores de tiempo.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: agregue aquí el código de dibujo
            spriteBatch.Begin();

            world.Draw();   // Dibujamos los objetos de la escena.

            ray.Draw(); // Dibujamos el rayo.

            // Dibujamos una cuadricula en pantalla:
            for (int x = 0; x < 800; x += 80)
                Helper.DrawLine(new Vector2(x, 0), new Vector2(x, 480), new Color(0, 255, 0, 128));
            for (int y = 0; y < 480; y += 80)
                Helper.DrawLine(new Vector2(0, y), new Vector2(800, y), new Color(0, 255, 0, 128));

            Helper.DrawText(onCollisionMessage, Vector2.Zero, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        Rectangle sensor;
        /// <summary>
        /// Simulamos la logica de gravedad y deteccion de suelo del cuerpo.
        /// </summary>
        /// <param name="owner"></param>
        public void PreUpdate(Body owner)
        {
            Body[] collideWith;
            world.IntersectRect(sensor, out collideWith);
            {
                int condition = 0;
                for (int i = 0; i < collideWith.Length; i++)
                    if (collideWith[i] != owner && collideWith[i].Solid && collideWith[i].Enabled)
                        condition++;

                if (condition == 0) owner.Rectangle.Y += 10;
            }
        }

        /// <summary>
        /// Actualizamos la vista del area que hace las funciones de sensor del cuerpo para detectar el suelo.
        /// </summary>
        /// <param name="owner"></param>
        public void PostUpdate(Body owner)
        {
            sensor.X = owner.Rectangle.X;
            sensor.Y = owner.Rectangle.Y + owner.Rectangle.Height;
            sensor.Width = owner.Rectangle.Width - 1;
            sensor.Height = 1;
        }

        /// <summary>
        /// Dibujamos el area del sensor.
        /// </summary>
        /// <param name="owner"></param>
        public void PostDraw(Body owner)
        {
            Helper.DrawBox(sensor, Color.Yellow);
        }

        /// <summary>
        /// Evento de colision del area de trigger.
        /// </summary>
        /// <param name="owner"></param>
        public void triggerOnCollision(Body[] b)
        {
            onCollisionMessage = "El cuerpo esta colisionando con el area de trigger.";
        }
    }
}
