using BulletSharp;
using BulletSharp.Math;
using Engine.Physics;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Менеджер физики
    /// </summary>
    public class PhysicsManager
    {
        /// <summary>
        /// Мир динамических объектов
        /// </summary>
        public DiscreteDynamicsWorld World { get; set; }
        /// <summary>
        /// Диспетчер коллизий
        /// </summary>
        public CollisionDispatcher dispatcher;
        /// <summary>
        /// хз пока что что это такое
        /// </summary>
        public DbvtBroadphase broadphase;
        /// <summary>
        /// Список коллизий
        /// </summary>
        public List<CollisionShape> collisionShapes = new List<CollisionShape>();
        /// <summary>
        /// Конфигурация коллизии
        /// </summary>
        public CollisionConfiguration collisionConf;
        private const int MaxThreadCount = 64;
        private readonly ConstraintSolverPoolMultiThreaded _constraintSolver;
        /// <summary>
        /// Основной конструктор
        /// </summary>
        public PhysicsManager()
        {
            // collision configuration contains default setup for memory, collision setup
            using (var collisionConfigurationInfo = new DefaultCollisionConstructionInfo
            {
                DefaultMaxPersistentManifoldPoolSize = 80000,
                DefaultMaxCollisionAlgorithmPoolSize = 80000
            })
                collisionConf = new DefaultCollisionConfiguration(collisionConfigurationInfo);

            dispatcher = new CollisionDispatcher(collisionConf);
            _constraintSolver = new ConstraintSolverPoolMultiThreaded(MaxThreadCount);
            broadphase = new DbvtBroadphase();
            World = new DiscreteDynamicsWorld(dispatcher, broadphase, _constraintSolver, collisionConf);
            World.Gravity = new Vector3(0, -9.81f, 0);
        }
        /// <summary>
        /// Обновление мира при каждом обновлении кадра
        /// </summary>
        /// <param name="elapsedTime"></param>
        public virtual void Update(float elapsedTime)
        {

            try
            {
                World.StepSimulation(elapsedTime);
            }
            catch(System.AccessViolationException)
            {
                System.Console.WriteLine("NEED TO FIX THIS SHIT");
            }
            /*foreach (var item in World.CollisionObjectArray)
            {
                World.DebugDrawObject(item.WorldTransform, item.CollisionShape, new Vector3(0, 255, 0));
            }*/
        }
        /// <summary>
        /// Выключение физики/закрытие окна
        /// </summary>
        public void ExitPhysics()
        {
            //remove/dispose constraints
            int i;
            for (i = World.NumConstraints - 1; i >= 0; i--)
            {
                TypedConstraint constraint = World.GetConstraint(i);
                World.RemoveConstraint(constraint);
                constraint.Dispose();
            }

            //remove the rigidbodies from the dynamics world and delete them
            for (i = World.NumCollisionObjects - 1; i >= 0; i--)
            {
                CollisionObject obj = World.CollisionObjectArray[i];
                RigidBody body = obj as RigidBody;
                if (body != null && body.MotionState != null)
                {
                    body.MotionState.Dispose();
                }
                World.RemoveCollisionObject(obj);
                obj.Dispose();
            }

            //delete collision shapes
            foreach (CollisionShape shape in collisionShapes)
                shape.Dispose();
            collisionShapes.Clear();

            World.Dispose();
            broadphase.Dispose();
            if (dispatcher != null)
            {
                dispatcher.Dispose();
            }
            collisionConf.Dispose();
        }
        /// <summary>
        /// Создание твердого тела
        /// </summary>
        /// <param name="mass">Масса тела</param>
        /// <param name="startTransform">Начальная матрица трансформации/позиции</param>
        /// <param name="shape">Вид коллизии</param>
        /// <returns></returns>
        public EngineRigidBody LocalCreateRigidBody(float mass, Matrix startTransform, CollisionShape shape)
        {
            bool isDynamic = (mass != 0.0f);

            Vector3 localInertia = Vector3.Zero;
            if (isDynamic)
                shape.CalculateLocalInertia(mass, out localInertia);

            DefaultMotionState myMotionState = new DefaultMotionState(startTransform);

            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(mass, myMotionState, shape, localInertia);
            EngineRigidBody body = new EngineRigidBody(rbInfo);

            World.AddRigidBody(body);

            return body;
        }
    }
}
