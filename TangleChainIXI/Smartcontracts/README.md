# TangleChain Assembler

## Instruction Table

Here are the instruction codes for the smartcontract assembler language. 
Numbers are internally stored as strings with a prefix ("Str_" or "Int_"), 
which lets the Computer determine what to do with the data. If you want to introduce Values, you need to add a prefix.


| Instruction  | Code | Args1  | Args2 | Args3 |
| :-------: | :-------: | :-------: | :-------: | :-------: |
| Copy  | 00  | Source | Destination | - |
| Introduce Value | 01  | Stringvalue | Destination |  |
| Add | 03 | Source1 | Source2 | Destination |
| Multiply | 04 | Source1 | Source2 | Destination |
| Set Entry | 05 | Name | - | - |
| Set State | 06 | Source | StateName | - |
| Comment | 08 | Comment | - | - |
| Set out Transaction | 09 | Receiver | Amount | - |
| Introduce State  | 10 | Name of state variable | Destination | - |
| Introduce MetaData  | 11 | Index | Destination | - |
| Subtract | 12 | Source1 | Source2 | Destination |
| Goto | 13 | Destination | - | - |
| Branch if Args1 is equal  Args2 | 14 | Destination | Arg1 | Arg2 |
| Introduce Data | 15 | index | destination | - |
| Branch if Args1 is not equal Args2 | 16 | destination | Args1 | Args2 |
| Branch if Args1 is lower then Args2 | 17 | destination | Args1 | Args2 |
| Switch Register | 18 | Args1 | Args2 | - |
| Jump and link | 19 | Destination | - | - |
| Pop and Jump | 20 | - | - | - |
| Branch if One | 21 | Destination | Args1 | - |
| Is Smaller | 22 | Args1 | Args2 | Destination for Flag |
| Is Bigger | 23 | Args1 | Args2 | Destination for Flag |
| Is Equal | 24 | Args1 | Args2 | Destination for Flag |
| AND | 25 | Args1 | Args2 | Destination for Flag |
| Negate | 26 | Source | - | - |
| OR | 27 | Args1 | Args2 | Destination for Flag |
| Exit Program | 99 | - | - | - |
|Label | 28 | - | - | - |

## State

The state variables are added to a smartcontract like this:

    smart.Code.AddVariable("Whatever");

State variables are updated and stored in a DB, this means you can store important stuff in them and they are persistent (Transaction #1 can store stuff in the state and then Transaction #2 can access it)

You need to use Instruction 10 to introduce a statevariable into the register and need to use instruction 06 to save the state changes.

## Registers

Registers are NOT persistent and are deleted once you finish processing the smartcontract.

## Math

```C#

1 + 1 = 2;
1 + "1" = "12";
"1" + "1" = "11";
1 * 2 = 2;
"2" * 3 = "222"; 
//Division doesnt exist

```

## Sending from a smartcontract

If you want to send from a smartcontract, you first need to have a balance on that smartcontract by just 
sending a normal transaction to a smartcontract OR sending a transaction with mode 2 to a smartcontract and triggering the smartcontract.

Use Instruction 09 to set the balance and address of the transaction.

## Transaction Modes

* -1 Genesis Transaction -> Always true, you can send as much coins as you want, because it will always be valid if the block height is 0
* 1 Normal Transaction
* 100 Outcoming Transaction from a smartcontract
* 2 A Transaction which can trigger a smartcontract

## Labels and Entrys

Entrys can be jumped at from a transaction. Labels can be reached by GOTO.

## Metadata Fields

MetaData is data from the trigger transaction such as Hash of the transaction, poolAddress etc. You can access them with providing an index as seen in the table.

An Expression like this returns the transaction time:


    new Expression(11,"Int_2");


| Hash | PoolAddress | Time | From | Incoming Balance |
| :-------: | :-------: | :-------: | :-------: | :-------: |
| Int_0 | Int_1 | Int_2 | Int_3 | Int_4 |

## Datafields

The Datafield of a transaction is data which the user can specify. It is index and can be access with the code from above (but instead of Instruction 11, use Instruction 15)

| Data[0] | Data[1] | Data[2] | Data[3] | ... | ... | 
| :-------: | :-------: | :-------: | :-------: | :-------: | :-------: |
| TransactionFee | The name of the entry function | _DATA[0] | _DATA[1] | _DATA[2] | ... |

## Example on how to run smartcontracts

```C#
//using strain nuget package to generate exp list
Smartcontract smart = new Strain(code).GenerateSmartcontract(SendTo); 
//setup transaction
var triggerTransaction = new Transaction();
//the computer where the smartcontract is run
var comp = new Computer(smart);

var resultTransaction = comp.Run(triggerTransaction);

//we extract the new state now
var state = comp.GetCompleteState();

//we update the smartcontract
smart.ApplyState(state);

//we run again, you can choose a different triggerTransaction too
comp = new Computer(smart);
var resultTransaction2 = comp.Run(triggerTransaction);
```
