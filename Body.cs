using Microsoft.Xna.Framework;

namespace CollisionTesting
{
    public class Body
    {
        /// <summary>
        /// Referencia a la instancia del mundo al que pertecene el cuerpo:
        /// </summary>
        public World WorldInstance { set; get; }

        public string Name;
        public string Tag = "";
        public Color Color;
        public bool Fixed;
        public bool Solid;
        public Rectangle Rectangle;
        public bool Enabled;
        public bool Debug;

        /// <summary>
        /// Establece o devuelve la posicion del objeto en el mundo que lo contiene.
        /// </summary>
        public Vector2 Location
        {
            get { return Helper.PointToVector2(Rectangle.Location); }
            set { Rectangle.Location = Helper.Vector2ToPoint(value); }
        }

        /// <summary>
        /// Establece o devuelve la posicion del objeto, desde su centro, en el mundo que lo contiene.
        /// </summary>
        public Vector2 Center
        {
            get { return Helper.PointToVector2(Rectangle.Center); }
            set { Rectangle.Location = new Point((int)(value.X - Rectangle.Width / 2), (int)(value.Y - Rectangle.Height / 2)); }
        }

        internal Vector2 lastPoint;

        /// <summary>
        /// Devuelve la direccion, angulo en grados, que toma el objeto en movimiento.
        /// </summary>
        public float Direction { get; internal set; }

        public bool Left { get; internal set; }
        public bool Right { get; internal set; }
        public bool Down { get; internal set; }
        public bool Up { get; internal set; }

        /// <summary>
        /// Delegado del evento de colsion.
        /// </summary>
        /// <param name="b">Lista de cuerpos con los que colisiona.</param>
        public delegate void CollisionHandler(Body[] b);

        /// <summary>
        /// Delegado para el metodo de pre actualizacion.
        /// </summary>
        /// <param name="owner">Referencia al objeto que lo contiene.</param>
        public delegate void PreUpdateHandler(Body owner);

        /// <summary>
        /// Delegado para el metodo post actualizacion.
        /// </summary>
        /// <param name="owner">Referencia al objeto que lo contiene.</param>
        public delegate void PostUpdateHandler(Body owner);

        /// <summary>
        /// Delegado para el metodo de pre dibujado.
        /// </summary>
        /// <param name="owner">Referencia al objeto que lo contiene.</param>
        public delegate void PreDrawHandler(Body owner);

        /// <summary>
        /// Delegado para el metodo de pre dibujado.
        /// </summary>
        /// <param name="owner">Referencia al objeto que lo contiene.</param>
        public delegate void PostDrawHandler(Body owner);

        /// <summary>
        /// Constructor de la clase.
        /// </summary>
        /// <param name="rectangle">Area del cuerpo.</param>
        /// <param name="color">Color con el que se representara.</param>
        /// <param name="isFixed">Indica si es fijo.</param>
        /// <param name="isSolid">Indica si es solido.</param>
        public Body(Rectangle rectangle, Color color, bool isFixed, bool isSolid)
        {
            Rectangle = rectangle;
            Color = color;
            Fixed = isFixed;
            Solid = isSolid;
            Enabled = true;
            Debug = false;

            lastPoint = this.Location;
            Direction = -1;             // -1 Significa que no se mueve.
        }

        public void Update()
        {
            if (!Fixed && (Rectangle.X != lastPoint.X || Rectangle.Y != lastPoint.Y))
            {
                // Direccion (angulo) desde la ultima posicion hasta la actual:
                Direction = MathTools.GetAngle(lastPoint, Helper.PointToVector2(Rectangle.Location));
            }
            else
                Direction = -1;

            // Indicamos las direcciones fijas segun el angulo de direccion del objeto:
            Left = Right = Up = Down = false;
            if (Direction > -1)
            {
                if (Direction == 0)
                    Right = true;
                else if (Direction == 90)
                    Down = true;
                else if (Direction == 180)
                    Left = true;
                else if (Direction == 270)
                    Up = true;
                else
                {
                    if (Direction > 0 && Direction < 90)
                        Right = Down = true;
                    else if (Direction > 90 && Direction < 180)
                        Down = Left = true;
                    else if (Direction > 180 && Direction < 270)
                        Left = Up = true;
                    else
                        Up = Right = true;
                }
            }

            // Almacenamos la ultima posicion:
            lastPoint = this.Location;
        }

        /// <summary>
        /// Dibuja la representacion grafica del cuerpo.
        /// </summary>
        public void Draw()
        {
            // Si se definio el metodo de pre dibujado lo ejecutamos:
            if (this.PreDraw != null) PreDraw(this);

            // Dibujamos el area del cuerpo:
            Helper.DrawBox(Rectangle, Color);

            // Escribimos el angulo:
            if (Debug)
            {
                string directions = (Left ? "Left\n" : "") + (Up ? "Up\n" : "") + (Right ? "Right\n" : "") + (Down ? "Down" : "");
                if (!Fixed) Helper.DrawText(Direction.ToString() + "º\n" + directions, new Vector2(Rectangle.X + 1, Rectangle.Y + 1), Color.White);
            }

            // Si no es un cuerpo fijo y este se ha movido en la ultima iteracion dibujamos una linea que represente la direccion:
            if (!Fixed && Direction > -1)
            {
                // Obtenemos el extremo de la linea desplazando las coordenadas xy 32 pixeles en la direccion recorrida:
                Vector2 endPoint = MathTools.Move(Center, 32, Direction);

                // Dibujamos la linea:
                Helper.DrawLine(Center, endPoint, Color.Blue);
            }

            // Si se definio el metodo de post dibujado lo ejecutamos:
            if (this.PostDraw != null) PostDraw(this);
        }

        /// <summary>
        /// Codigo que se ejecutara antes de actualizar el objeto.
        /// </summary>
        public PreUpdateHandler PreUpdate;

        /// <summary>
        /// Codigo que se ejecutara despues de actualizar el objeto.
        /// </summary>
        public PostUpdateHandler PostUpdate;

        /// <summary>
        /// Codigo que se ejecutara antes de dibujar el objeto.
        /// </summary>
        public PreUpdateHandler PreDraw;

        /// <summary>
        /// Codigo que se ejecutara despues de dibujar el objeto.
        /// </summary>
        public PostUpdateHandler PostDraw;

        /// <summary>
        /// Evento de colision.
        /// </summary>
        public CollisionHandler OnCollision;

    }
}