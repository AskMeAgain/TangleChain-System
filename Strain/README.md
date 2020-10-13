# Strain

Strain is a programming language designed for the Assembly language of TangleChainIXI. You can write high level smartcontracts for the TangleChain in a familiar syntax.


## How to compile to smartcontract object

Import the Strain Nuget Package to your project and just pass your code as a string to the Strain Object and call GenerateSmartcontract(*name of sendto addr*). 
The object will generate a smartcontract where all the statevars and expressions are correctly set.

```C#
Smartcontract smart = new Strain(code).GenerateSmartcontract(SendTo);
```

## How to call a function

You need to send a transaction to the smartcontract address with mode set to 2. The transaction needs to be structured like this:

| Data[0] | Data[1] | Data[2] | Data[3] | ... | ... | 
| :-------: | :-------: | :-------: | :-------: | :-------: | :-------: |
| TransactionFee | The name of the entry function | _DATA[0] | _DATA[1] | _DATA[2] | ... |


## Introduction

The language doesnt have any types. They are converted on the fly. This means you dont need to specify a type when you declare a variable or return anything from a function. Strings are in "" and numbers are ALWAYS integers.

The language is designed in such a way that you specify the functions which a user can call. These functions are called entry functions and are marked as such. The base structure is always the same:

## General Structure

```C#
ApplicationName {
    entry UserCanCallThisFunction(){
    }
    entry UserCanCallThisFunctionToo(){
    }
    function UsersCantCallthisFunction(){
    }
}
```
You can specify if entry functions can take parameters (eg the user can submit a number to vote for something). You can ofc use these parameters for branching etc

```C#
Vote {
    entry Vote(vote){
        if(vote == "test"){
            //parameter can be used
        }
    }
}
```

## Basic

Variables are declared like this

```C#
Test {
    entry Test(vote){
        var isAnInt = 3;
        var isAString = "3";
        var isAnArray[3]; //array needs to specify the array size first
        isAnArray[0] = 1;
        isAnArray[1] = "1"; //no problem with double type!
    }
}

```

Correct math is also supported, but no brackets (1 * (2+3), doesnt work but 1 + 2 * 3 is working)

```C#
Test {
    entry Test(vote){
        var number = 1 + 2 * 3; //correctly stores 7
    }
}

```

## Branching

You can branch and use "else" statements.

```C#
Test {
    entry Test(vote){
        if(1 == 1){
            //will reach this since 1 == 1
        }else{
            //never reached
        }
        if(1 < 3 && 3 >= 3){
            //is also supported
        }
        if(1 > 3 || 3 > 1){
            //also supported
        }
    }
}

```

## Functions

You can call functions like so and add comments with // (which are not compiled). Each function MUST have a return statement. You dont need to return anything useful so if you have a void function just return 0; in the end.

```C#
Vote {
    entry Vote(vote){
        doStuff(vote);
    }
    function doStuff(vote){
        //do some stuff here
        return 0; //is needed!
    }
}
```

## Special Functions

There some special functions which are ALWAYS written in Capslock and beginning with an underscore (_):

```C#
_LENGTH(arrayName); //returns the length of the array

_META[index]; //returns the meta values of a transaction

_DATA[index]; //access the datafield of a transaction

_OUT(address,balance); //creates an out transaction so a smartcontract can actually send some money.
```


## State Variables

If you want to store some data in a smartcontract (eg some vote inside a multisignature wallet for example), you need to declare state variables. Below is an example for a counter smartcontract which just counts up whenever the function is triggered and once it reaches 10 it resets to 0:

```C#
Counter {
    state counter;
    entry CountUp(){
        counter = counter + 1;
        if(counter == 10){
            counter = 0;
        }
    }
}
```

An array can actually also be a statevar, but its important to note that they are converted to single statevars (eg var array[3]; will compile to 3 statevars: array_0, array_1 and array_2_), so dont be to generous with them


# Multisignature example

Here is a multisignature example where users need to vote on the same address and same balance to actually send some funds out of the smartcontract
```C#
Multisignature {

    //the state vars
    var users[2];
    var votes[2];
    var balances[2];

    //inits the smartcontract and sets the users. Note that i can actually call init again just to mimic some ethereum smartcontracts
    entry Init(u1,u2){
        votes[0] = 0;
        votes[1] = 0;
        balances[0] = 0;
        balances[1] = 0;
        users[0] = u1;
        users[1] = u2;
    }

    //each user can call this function but only if you are registered it will be saved
    entry Vote(to,balance){
        intro i = 0;
        while(i < _LENGTH(users)){
            //checking if the user is allowed to vote
            if(users[i] == _META[3]){
                votes[i] = to;
                balances[i] = balance;
            }
            i = i + 1;
        }
    }

    //final trigger needs to be made to send the transaction. You could actually remove this and include it in the vote function if you want
    entry Send(){
        if(votes[0] == votes[1] && balances[0] == balances[1]){
            //sending out the transaction
            _OUT(balances[0],votes[1]);
            //reseting the vote
            votes[0] = 0;
            votes[1] = 0;
        }
    }
}
```
