# TangleChain-System

The TangleChain-System is a collection of tools/programs to host a blockchain on top of **Iota**, each written in **C#**.

### TangleChain-IXI Library v1

TangleChain-IXI is the heart of the TangleChain-System. It is a **.Net Standard** Library to interact with the TangleChain-Blockchain on top of the [IOTA Tangle](https://github.com/iotaledger). It is planned to have Smartcontracts and interoperability between these chains. The planned blocktime is around 1-2 Minutes.

### Console Miner v0.9

This Miner is a console application which is designed to run on a server. It is currently in Beta (it works, but could have some bugs somewhere).

### Other

There are lots of other projects planned: A Block explorer, a Wallet and more.

## Current Features of IXI Library

- Complete on Tangle blockchain
- Local database storage using **SQLite**
- Longest chain detection
- Account System (public private key using **Nethereum**)
- Easy start of a chain with **ConsoleMiner**
- Proof-of-Work Consensus in a transition phase
- NuGet Package (TangleChainIXI)

## Missing Features

- Difficulty System to ensure blocktime of 1-2 minutes
- Interoperability between different chains
- Smart contracts
- Environment friendly consensus protocol
- Complete Documentation
- Whitepaper (maybe my Bachlor Thesis who knows)


### Getting Started with IXI

The best way to get started is to check the unit tests of the IXI Library. Checkout  TestInits/Initalizing.cs
There is a NuGet package, just search for "TangleChainIXI".


### Getting Started with the Console ConsoleMiner

The console miner needs an init.json file where it takes all the settings. Just run with the init parameter, so an init file is created *ConsoleMiner.exe init*.
*ConsoleMiner.exe run* just runs the console miner based on the init file. If you want to create a new chain, use *ConsoleMiner.exe run genesis*. Publish your chainsettings (eg: chainname, hash, address) and others can mine your chain too when they enter the information into the init.json file.


###Donation

If you want to support this project with some iotas, below is the donation address. The money is most likely used to pay for server. Thank you for reading

XUAKJUNNUOUXMRBLDEDHNLCPBCQGVXCSPUTHMLJJTMGHOCBYAYVTQROYMFPMMOLMT9JAHARFCLKKWWBX9MNBHW9NRD
