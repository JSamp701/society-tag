gfile format
----------------------------------------
<int taggedPenalty>,<int tagsBonus>,<int numAlive>
<string name>,<string email>,<int id>,<int target>,<int hunter>,<bool alive>,<int tags>,<bool disqualified>
. . . . .

Git process
----------------------------------------
git branch <featurename>
git checkout <featurename>
---dowork---
---commitwork---
git checkout master
git pull
git merge master <featurename>
git push