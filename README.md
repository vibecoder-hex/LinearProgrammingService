# Linear Programming Service.
Simplex method solving program

#Features

1. Constructing equatations and target functions from coeffecients and constrants vectors
2. Building simplex table with uppers and lower bounds
3. Table transformation and found optimal solution

#TechStack
1.C#(.NET 10.0)
2. Spectre.Console(for pretty data prining)
```
> ./SimplexMethod 3 3
P1: 1 2 3
P2: 4 5 6
P3: 7 8 9
B: 20 30
Select constraint type for 20: 
 1.Equal, 2.LessOrEqual, 3.GreaterOrEqual
2
Select constraint type for 30: 
 1.Equal, 2.LessOrEqual, 3.GreaterOrEqual
2

Function vector:
F: 1X1 + 4X2 + 7X3
Condition matrix:
2X1 + 5X2 + 8X3 <= 20
3X1 + 6X2 + 9X3 <= 30


2,000|0,000 5,000|0,000 8,000|0,000 20,000|0,000 
3,000|0,000 6,000|0,000 9,000|0,000 30,000|0,000 
-1,000|0,000 -4,000|0,000 -7,000|0,000 0,000|0,000 

0,000|0,000 0,000|0,000 0,875|0,000 0,000|0,000 
0,000|0,000 -3,375|0,000 -3,375|0,000 -10,125|0,000 
0,250|0,000 0,250|0,000 0,250|0,000 0,500|0,000 

F = 0,500
```
