# LinqToSoql

This is linq provider for [salesforce object query language](http://www.salesforce.com/us/developer/docs/soql_sosl/)

###Possibilities
 - Usual select (you should specify all fields you want to be selected)
 - Direct access to lookup field (ChildToParentRelationShip)
 - ParentToChildRelationShip
 - Where with base comparison and logical operators
 - LIKE operator: you can use StartsWith, EndsWith, Contains and Like(extention) methods with string
 - IN and NOT IN

###ToDo's
 - SOQL comparison operators: INCLUDES, EXCLUDES
 - fix nullable convertation
 - OrderBy
