using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EmitRadiusBurst(Vector3 position, int quantity, GameObject particle_prefab, Vector3 cone_angle, Vector3 cone_radius)
    {
        for (int i = 0; i < quantity; i++)
        {
            GameObject part_obj = Instantiate(particle_prefab, position, Quaternion.identity);
            IParticle part = part_obj.GetComponent<IParticle>();

            part.direction = cone_angle;
            part.direction += new Vector3(  Random.Range(-cone_radius.x/2, cone_radius.x/2), 
                                            Random.Range(-cone_radius.y/2, cone_radius.y/2), 
                                            Random.Range(-cone_radius.z/2, cone_radius.z/2));
        }
    }

    public GameObject EmitSingleParticle(Vector3 position, GameObject particle_prefab, Vector3 direction, float speed = -1f)
    {
        GameObject part_obj = Instantiate(particle_prefab, position, Quaternion.identity);
        IParticle part = part_obj.GetComponent<IParticle>();

        part.direction = direction;
        if (speed != -1f) part.speed = speed;

        return part_obj;
    }
}
