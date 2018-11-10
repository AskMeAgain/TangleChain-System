### TangleChain Assembler

Here are the instruction codes for the smartcontract assembler language

| Instruction  | Code | Args1  | Args2 | Args3 |
| :-------: | :-------: | :-------: | :-------: | :-------: |
| Copy  | 00  | Source | Destination | - |
| Introduce Value | 01  | Variable | Destination |  |
| Add | 03 | Source1 | Source2 | Destination |
| Multiply | 04 | Source1 | Source2 | Destination |
| Set Entry | 05 | Name | - | - |
| Set State | 06 | Source | Destination | - |
| Comment | 08 | Comment | - | - |
| Set out Transaction | 09 | Receiver | Amount | - |
| Copy state into Register  | 10 | Name of state variable | destination | - |
| Copy Metadata into Register  | 11 | Name of metadata | destination | - |
| Subtract | 12 | Source1 | Source2 | Destination |
| Goto | 13 | Destination | - | - |
| Branch if equal | 14 | Destination | Arg1 | Arg2 |


# How to access each data field

### Metadata Fields

| Hash | PoolAddress | Time | From |
| :-------: | :-------: | :-------: | :-------: |
| T_0 | T_1 | T_2 | T_3 |

### Data field of Transaction

D_X beginning on D_0

### State

The state variables are defined like this:

    smart.Code.AddVariable("Whatever");

State variables are updated and stored in a DB, this means you can store important stuff in them and they are persistent (Transaction #1 can store stuff in the state and then Transaction #2 can access it)

you can access them like this:

    S_Whatever

### Registers

Registers are NOT persistent and are deleted once you finish processing the smartcontract.

Access them like this:

    R_1
    R_1000
    R_332

### Transaction Modes

* -1 Genesis Transaction -> Always true, you can send as much coins as you want
* 1 Normal Transaction
* 100 Outcoming Transaction from a smartcontract
* 2 A Transaction which can trigger a smartcontract
