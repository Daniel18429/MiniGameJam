Overview of general bullet system

1) Bullet Pooling
Bullets are not instantiated, instead they are just turned on and off and given new data. This stops unity from having
to deal with the performance costs of creating and deleting a ton of bullets all the time

2) Bullet Behaviours
Instead of bullets all having monobehaviours that do things (horrible for performance when you have 10k bullets) the
bulletmanager stores a list of the behaviours, and then calls them on behalf of the bullet. Bullet Behaviours are a member
of the class IBulletBehaviour which are STATELESS meaning any instance of data will be shared across all bullets using
a reference to that instance of the interface in a given BulletBlueprint. To still allow for unique variables for each
bullet, the bulletmanager also maintains arrays for unique bullet data. If you need to give an IBulletInterface unique
data per bullet, make a data type with the name of the interface followed by "Data". Make sure you create a list of that
datatype in BulletManager. And ***make sure you swap the data of that list in RefactorBullet***

3) Bullet Spawner
Still being worked on but I'll probably just leave it as a MonoBehaviour and make a BulletSpawnerManager that you call
with the data necessary to spawn a custom BulletSpawner.
