using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    private List<ObjectPooler> poolers;

    // Start is called before the first frame update
    void Start()
    {
        poolers = new List<ObjectPooler>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EmitRadiusBurst(Vector3 position, int quantity, GameObject particle_prefab, Vector3 cone_angle, Vector3 cone_radius)
    {
        if (!GameManager.GetEffectsOn()) return;

        for (int i = 0; i < quantity; i++)
        {
            Vector3 dir = cone_angle;
            dir += new Vector3(  Random.Range(-cone_radius.x/2, cone_radius.x/2), 
                                            Random.Range(-cone_radius.y/2, cone_radius.y/2), 
                                            Random.Range(-cone_radius.z/2, cone_radius.z/2));

            EmitSingleParticle(position, particle_prefab, dir);
        }
    }

    public void EmitRadiusBurst(Vector3 position, int quantity, GameObject particle_prefab, Vector3 cone_angle, Vector3 cone_radius, float speed, Transform parent)
    {
        if (!GameManager.GetEffectsOn()) return;

        for (int i = 0; i < quantity; i++)
        {
            Vector3 dir = cone_angle;
            dir += new Vector3(  Random.Range(-cone_radius.x/2, cone_radius.x/2), 
                                            Random.Range(-cone_radius.y/2, cone_radius.y/2), 
                                            Random.Range(-cone_radius.z/2, cone_radius.z/2));

            EmitSingleParticle(position, particle_prefab, dir, speed, parent);
        }
    }

    public void EmitExplosion(Vector3 position, int quantity, GameObject particle_prefab)
    {
        for (int i = 0; i < quantity; i++)
        {
            Vector3 dir = new Vector3(Random.Range(-180f, 180f), Random.Range(-180f, 180f), Random.Range(-180f, 180f));

            EmitSingleParticle(position, particle_prefab, dir);
        }
    }

    public GameObject EmitSingleParticle(Vector3 position, GameObject particle_prefab, Vector3 direction, float speed = -1f, Transform parent = null)
    {
        ObjectPooler pooler = GetPooler(particle_prefab);

        GameObject part_obj = pooler.GetObject(position, Quaternion.identity);
        if (parent != null) part_obj.transform.SetParent(parent);

        IParticle part = part_obj.GetComponent<IParticle>();

        part.direction = direction;
        if (speed != -1f) part.speed = speed;

        return part_obj;
    }

    private ObjectPooler GetPooler(GameObject prefab)
    {
        //checks if there is a pooler for the particle prefab
        ObjectPooler pooler = null;
        for (int i = 0; i < poolers.Count; i++)
        {
            if (poolers[i].prefab == prefab)
            {
                pooler = poolers[i];
                break;
            }
        }

        //if there is no pooler for the particle prefab, create one
        if (pooler == null)
        {
            pooler = new ObjectPooler(prefab,null,20,1.5f);
            poolers.Add(pooler);
        }

        return pooler;
    }
}
