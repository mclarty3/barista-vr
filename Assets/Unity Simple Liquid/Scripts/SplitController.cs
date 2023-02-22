#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitySimpleLiquid
{
    /// <summary>
    /// Calculates liquids splitting effect and transfer to other liquid containers
    /// </summary>
    public class SplitController : MonoBehaviour
    {
		[Header("Container Settings")]
        public LiquidContainer liquidContainer;
        [SerializeField]
        private float bottleneckRadius = 0.1f;
        public float BottleneckRadiusWorld { get; private set; }

		[Header("Liquid Source Settings")]
		[Tooltip("The point from which the liquid will be poured. Only used if liquidContainer is a liquid source")]
		public Transform pourPoint;
		[Tooltip("Determines whether the liquid will be poured automatically when the pour point reaches a certain angle")]
		public bool angleBasedPour = false;
		[Tooltip("The angle at which the liquid will be poured. Only used if angleBasedPour is true")]
		public float pourAngle = 45f;
		[Tooltip("Toggles manual pouring. Only used if angleBasedPour is false and liquidContainer is a liquid source")]
		public bool manualPour = false;
		[Tooltip("The size of the splash when the liquid is poured manually. Only used if manualPour is true")]
		public float manualSplashSize = -1;

        [Header("\"Split\" Settings")]
        [Tooltip("How fast liquid split from container")]
        public float splitSpeed = 2f;
		[Tooltip("Number number of objects the liquid will hit off and continue flowing")]
		public int maxEdgeDrops = 4;
		[Tooltip("Multiplier for the delay between liquid leaving this container and being transferred to another")]
		public float transferLiquidDelayMultiplier = 1.2f;
		private int currentDrop;
		public ParticleSystem particlesPrefab;

		[Header("Pour position settings")]
		[Tooltip("The horizontal offset of the pour point relative to the rim of the bottleneck")]
		public float pourOffsetHorizontal = 0f;
		[Tooltip("The vertical offset of the pour point relative to the rim of the bottleneck")]
		public float pourOffsetVertical = 0f;


        #region Particles
        private ParticleSystem particles;
        public ParticleSystem Particles
        {
            get
            {
                if (!particlesPrefab)
                    return null;

                if (!particles)
                    particles = Instantiate(particlesPrefab, transform);
                return particles;
            }
        }

		public void Start()
		{
			if (liquidContainer == null)
			{
				TryGetComponent(out liquidContainer);
			}
		}

        private void StartEffect(Vector3 splitPos, float scale)
        {
            var particlesInst = Particles;
            if (!particlesInst)
                return;

            var mainModule = particlesInst.main;
            mainModule.startColor = liquidContainer.LiquidColor;

			var trailModule = particlesInst.trails;
			var alphaColor = liquidContainer.LiquidColor;
			alphaColor.a = 0.4f;
			trailModule.colorOverTrail = alphaColor;

            particlesInst.transform.localScale = Vector3.one * BottleneckRadiusWorld * scale;
            particlesInst.transform.position = splitPos;

			if (pourPoint == null)
				particlesInst.transform.rotation = Quaternion.LookRotation(bottleneckPlane.normal);

            particlesInst.Play();
        }
        #endregion

        #region Bottleneck
        public Plane bottleneckPlane { get; private set; }
        public Plane surfacePlane { get; private set; }
        public Vector3 BottleneckPos { get; private set; }

        private Plane GenerateBottleneckPlane()
        {
            if (!liquidContainer)
                return new Plane();

			if (liquidContainer.LiquidSource)
				return new Plane();

            var mesh = liquidContainer.LiquidMesh;
            if (!mesh)
                return new Plane();

            var max = mesh.bounds.max.y;
            return new Plane(liquidContainer.transform.up,
                max * liquidContainer.transform.lossyScale.y);
        }

        private Vector3 GenerateBottleneckPos()
        {
            if (!liquidContainer)
                return Vector3.zero;

            var tr = liquidContainer.transform;
            var pos = bottleneckPlane.normal * bottleneckPlane.distance + tr.position;
            return pos;
        }

		private Vector3 GenerateBottleneckLowesPoint()
		{
			if (!liquidContainer)
				return Vector3.zero;

			// Calculate the direction vector of the bottlenecks slope

			Vector3 bottleneckSlope = GetSlopeDirection(Vector3.up, bottleneckPlane.normal);

			// Find a position along the slope the side of the bottleneck radius
			Vector3 min = BottleneckPos + bottleneckSlope * BottleneckRadiusWorld;

			min += pourOffsetHorizontal * (min - BottleneckPos).normalized;
			min += pourOffsetVertical * transform.up;

			return min;

		}
		#endregion

		#region Gizmos
		private void OnDrawGizmosSelected()
        {
			if (liquidContainer && liquidContainer.LiquidSource)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(pourPoint.position, BottleneckRadiusWorld);

				if (IsSpliting)
				 	Gizmos.color = Color.green;

				Gizmos.DrawLine(pourPoint.position, pourPoint.position + pourPoint.up * 0.1f);
			}

            // Draws bottleneck direction and radius
            var bottleneckPlane = GenerateBottleneckPlane();
            BottleneckRadiusWorld = bottleneckRadius * transform.lossyScale.magnitude;

            Gizmos.color = Color.red;
            GizmosHelper.DrawPlaneGizmos(bottleneckPlane, transform);

            // And bottleneck position
            GizmosHelper.DrawSphereOnPlane(bottleneckPlane, BottleneckRadiusWorld, transform);

			var lowestPoint = GenerateBottleneckLowesPoint();
			Gizmos.DrawWireSphere(lowestPoint, manualSplashSize > 0 ? manualSplashSize : 0.025f);
		}
		private void OnDrawGizmos()
		{
			// Draw a yellow sphere at the transform's position
            if (raycasthit != Vector3.zero)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(raycasthit, 0.01f);
				Gizmos.DrawSphere(raycastStart, 0.01f);
			}
		}
		#endregion

		#region Split Logic
		private const float splashSize = 0.025f;

        public bool IsSpliting { get; private set; }

        private void CheckSpliting()
        {
            IsSpliting = false;

            if (liquidContainer == null)
                return;

			if (manualPour)
			{
				SplitLogic(pourPoint.position, liquidContainer.LiquidSource);
			}

			if (liquidContainer.LiquidSource)
			{
				if (angleBasedPour && CheckLiquidSourceSpill())
				{
					SplitLogic(pourPoint.position, liquidSource: true);
				}
			}

            // Do we have something to split?
            if (liquidContainer.FillAmountPercent <= 0f)
                return;
            if (!liquidContainer.IsOpen)
                return;

            // Check if liquid is overflows
            var overflows = GeomUtils.PlanePlaneIntersection(out Vector3 overflowsPoint, out _,
                bottleneckPlane, surfacePlane);

            // Translate to contrainers world position
            overflowsPoint += liquidContainer.transform.position;

            if (overflows)
            {
                // Let's check if overflow point is inside bottleneck radius
                var insideBottleneck = Vector3.Distance(overflowsPoint, BottleneckPos) < BottleneckRadiusWorld;

                if (insideBottleneck)
                {
                    // We are inside bottleneck - just start spliting from lowest bottleneck point
                    var minPoint = GenerateBottleneckLowesPoint();
                    SplitLogic(minPoint);
                    return;
                }
            }

            if (BottleneckPos.y < overflowsPoint.y)
            {
                // Oh, looks like container is upside down - let's check it
                var dot = Vector3.Dot(bottleneckPlane.normal, surfacePlane.normal);
                if (dot < 0f)
                {
                    // Yep, let's split from the bottleneck center
                    SplitLogic(BottleneckPos);
                }
                else
                {
                    // Well, this weird, let's check if spliting point is even inside our liquid
                    var dist = liquidContainer.liquidRender.bounds.SqrDistance(overflowsPoint);
                    var inBounding = dist < 0.0001f;

                    if (inBounding)
                    {
                        // Yeah, we are inside liquid container
                        var minPoint = GenerateBottleneckLowesPoint();
                        SplitLogic(minPoint);
                    }
                }
            }
        }

		private bool CheckLiquidSourceSpill()
		{
			if (pourPoint == null)
				return false;

			// if (Vector3.Dot(pourPoint.transform.up, Vector3.down) < 0.5f)
			if (Vector3.Angle(pourPoint.transform.up, Vector3.down) > pourAngle)
				return false;

			return true;
		}

        private void SplitLogic(Vector3 splitPos, bool liquidSource = false)
        {
            IsSpliting = true;

            // Check rotation of liquid container
            // It conttolls how many liquid we lost and particles size
            var howLow = Vector3.Dot(Vector3.up, liquidContainer.transform.up);
            var flowScale = 1f - (howLow + 1) * 0.5f + 0.2f;

            var liquidStep = BottleneckRadiusWorld * splitSpeed * Time.deltaTime * flowScale;

            // Only change this container's volume if it's not a liquid source
            if (!liquidSource)
			{
				var newLiquidAmmount = liquidContainer.FillAmountPercent - liquidStep;

				// Check if amount is negative and change it to zero
				if (newLiquidAmmount < 0f)
				{
					liquidStep = liquidContainer.FillAmountPercent;
					newLiquidAmmount = 0f;
				}

				// Transfer liquid to other container (if possible)
				liquidContainer.FillAmountPercent = newLiquidAmmount;

				TryGetComponent(out IngredientManager ingredientManager);
				ingredientManager?.SplitLiquid(liquidStep);
			}

			//RaycastHit is a struct which gives us everything we need
			RaycastHit containerHit = FindLiquidContainer(splitPos, gameObject, out var splitController);

			if (splitController != null || containerHit.collider != null)
			{
				if (splitController == null)
				{
					splitController = containerHit.collider.GetComponent<SplitController>();
				}

				float distanceToContainer = Vector3.Distance(splitPos, splitController.transform.position);
				// use gravity to calculate time to reach container
				float gravity = Physics.gravity.magnitude * Particles.main.gravityModifierMultiplier;
				float timeToReachContainer = Mathf.Sqrt(2 * distanceToContainer / gravity) * transferLiquidDelayMultiplier;
				StartCoroutine(DelayTransferLiquid(containerHit, liquidStep, flowScale, liquidSource, splitController, timeToReachContainer));

			}
			// Start particles effect
			StartEffect(splitPos, flowScale);
		}

		IEnumerator DelayTransferLiquid(RaycastHit hit, float lostPercentAmount, float scale, bool liquidSource,
										SplitController liquid, float delay)
		{
			yield return new WaitForSeconds(delay);

			TransferLiquid(hit, lostPercentAmount, scale, liquidSource, liquid);
		}

		//Used for Gizmo only
		private Vector3 raycasthit;
		private Vector3 raycastStart;

		private void TransferLiquid(RaycastHit hit, float lostPercentAmount, float scale, bool liquidSource)
        {
			var liquid = hit.collider.GetComponent<SplitController>();

			TransferLiquid(hit, lostPercentAmount, scale, liquidSource, liquid);
		}

		private void TransferLiquid(RaycastHit hit, float lostPercentAmount, float scale, bool liquidSource,
								    SplitController liquid)
		{
			var otherBottleneck = liquid.GenerateBottleneckPos();
			var radius = liquid.BottleneckRadiusWorld;

			var hitPoint = hit.point;

			// Does we touched bottleneck?
			var horizontalDistance = Vector3.ProjectOnPlane(hitPoint - otherBottleneck, transform.up).magnitude;
			var insideRadius = horizontalDistance < radius + (manualSplashSize != -1 ? manualSplashSize : splashSize) * scale;
			if (insideRadius)
			{
				var lostAmount = lostPercentAmount;
				if (!liquidSource)
					lostAmount *= liquidContainer.Volume;

				liquid.liquidContainer.FillAmount += lostAmount;

				//color change in capacity
				SendLiquidContainer(liquid.liquidContainer);

				TryGetComponent<IngredientManager>(out var ingredientManager);
				liquid.liquidContainer.TryGetComponent<IngredientManager>(out var otherIngredientManager);

				if (ingredientManager && otherIngredientManager)
				{
					otherIngredientManager.AddLiquid(ingredientManager, lostAmount);
				}
			}

		}

		private RaycastHit FindLiquidContainer(Vector3 splitPos, GameObject ignoreCollision, out SplitController hitObject)
		{

			hitObject = null;
			var ray = new Ray(splitPos, Vector3.down);

			// Check all colliders under ours
			var hits = Physics.SphereCastAll(ray, manualSplashSize != -1 ? manualSplashSize : splashSize);
			hits = hits.OrderBy((h) => h.distance).ToArray();

			var childrenColliders = ignoreCollision.GetComponentsInChildren<Collider>();

			foreach (var hit in hits)
			{
				// Debug.Log(hit.collider.gameObject);
				//Ignore ourself
				if (ReferenceEquals(hit.collider.gameObject, ignoreCollision) ||
				 	childrenColliders.Contains(hit.collider) ||
					hit.collider.isTrigger)
				{
					continue;
				}

				// does it even a split controller
				var liquid = hit.collider.GetComponent<SplitController>();

				if (!liquid) {
					// If we directly hit a LiquidCollider, get its parent's SplitController
					if (hit.collider.gameObject.tag == "LiquidCollider") {
						liquid = hit.collider.transform.parent.parent.GetComponent<SplitController>();
						hitObject = liquid;
					}
				}

				if (liquid && liquid != this)
				{


					return hit;
				}


				//Something other than a liquid splitter is in the way
				if (!liquid)
				{
					//If we have already dropped down off too many objects, break

					if (currentDrop >= maxEdgeDrops)
					{
						//if we have rolled down too many objects, return empty hit
						//This assumes at this point the liquid has "dried up" rather than pouring from the last valid point
						return new RaycastHit();
					}
					//Simulate the liquid running off an object it hits and continuing down from the edge of the liquid
					//Does not take velocity into account

					//First get the slope direction
					Vector3 slope = GetSlopeDirection(Vector3.up, hit.normal);

					//Next we try to find the edge of the object the liquid would roll off
					//This really only works for primitive objects, it would look weird on other stuff
					Vector3 edgePosition = TryGetSlopeEdge(slope, hit);
					if (edgePosition != Vector3.zero)
					{
						//edge position found, surface must be tilted
						//Now we can try to transfer the liquid from this position
						currentDrop++;
						return FindLiquidContainer(edgePosition, hit.collider.gameObject, out hitObject);

					}
					return new RaycastHit();
				}
			}
			return new RaycastHit();
		}

		#endregion

		#region ChangeColor
		//playerMultiply for faster color change
		[Range(0, 2)]
		[Tooltip("Mixing speed ratio of different colors")]
		public float mixingSpeed = 1;

		private void SendLiquidContainer(LiquidContainer lc)
		{
			//find the color and split speed
			Color newColor = liquidContainer.LiquidColor;
			float ss = liquidContainer.GetSplitController.splitSpeed;

			//we find the coefficient of the volume of the tank and the volume of the incoming fluid
			float volume = lc.Volume;
			float koof = ss / (volume * 1000);
			lc.LiquidColor = Color.Lerp(lc.LiquidColor, newColor, koof * mixingSpeed);
		}
		#endregion

		#region Slope Logic
		private float GetIdealRayCastDist(Bounds boundBox, Vector3 point, Vector3 slope)
		{
			Vector3 final = boundBox.min;

			// X axis
			if (slope.x > 0)
				final.x = boundBox.max.x;
			// Y axis
			if (slope.y > 0)
				final.y = boundBox.max.y;
			// Z axis
			if (slope.z > 0)
				final.z = boundBox.max.z;

			return Vector3.Distance(point, final);
		}

		private Vector3 GetSlopeDirection(Vector3 up, Vector3 normal)
		{
			//https://forum.unity.com/threads/making-a-player-slide-down-a-slope.469988/#post-3062204
			return Vector3.Cross(Vector3.Cross(up, normal), normal).normalized;
		}

		private Vector3 TryGetSlopeEdge(Vector3 slope, RaycastHit hit)
		{
			Vector3 edgePosition = Vector3.zero;

			// We need to pick a position outside of the object to raycast back towards it to find an edge.
			// We need a position slightly down so it will hit the edge of the object
			Vector3 moveDown = new Vector3(0f, -0.0001f, 0f);
			// We also need to move the position outside of the objects bounding box, so we actually hit it
			float dist = GetIdealRayCastDist(hit.collider.bounds, hit.point, slope);

			Vector3 reverseRayPos = hit.point + moveDown + (slope * dist);
			raycastStart = reverseRayPos;
			Ray backwardsRay = new Ray(reverseRayPos, -slope);
			RaycastHit[] revHits = Physics.RaycastAll(backwardsRay);

			foreach (var revHit in revHits)
			{
				// https://answers.unity.com/questions/752382/how-to-compare-if-two-gameobjects-are-the-same-1.html
				//We only want to get this position on the original object we hit off of
				if (GameObject.ReferenceEquals(revHit.collider.gameObject, hit.collider.gameObject))
				{
					//We hit the object the liquid is running down!
					raycasthit = edgePosition = revHit.point;
					break;
				}
			}
			return edgePosition;
		}
		#endregion

		private void Update()
        {
            // Update bottleneck and surface from last update
            bottleneckPlane = GenerateBottleneckPlane();
            BottleneckPos = GenerateBottleneckPos();
            surfacePlane = liquidContainer.GenerateSurfacePlane();
            BottleneckRadiusWorld = bottleneckRadius * transform.lossyScale.magnitude;

			// Now check spliting, starting from the top
			currentDrop = 0;
            CheckSpliting();
        }
    }
}
