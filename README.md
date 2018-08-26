# TangleChain-System

The TangleChain-System is a collection of tools/programs to host a blockchain on top of **Iota**, each written in **C#**.

___________________________________________________


### TangleChain-IXI Library v1.2

TangleChain-IXI is the heart of the TangleChain-System. It is a **.Net Standard** Library to interact with the TangleChain-Blockchain on top of the [IOTA Tangle](https://github.com/iotaledger). It is planned to have Smartcontracts and interoperability between these chains. The planned blocktime is around 1-2 Minutes.



#### IXI v1.2 Features:

- Reworked the Database system and included a DBManager. You can only access DB stuff from the new Manager
- Complete on Tangle blockchain
- Local database storage using **SQLite**
- Dynamic Difficulty adjustment as specified in the genesis Block
- Account System (public private key using **Nethereum**)
- Proof-of-Work Consensus in a transition phase
- NuGet Package (TangleChainIXI)


#### IXI v1.2 Missing Features:

- Smartcontracts
- Interoperability
- Detect longest Chain via Blockweight
- Environment friendly consensus protocol
- Complete Documentation
- Whitepaper

___________________________________________________

### Console Miner v1

This Miner is a console application which is designed to run on a server. It is currently in Beta.

#### Features of Console miner

- Easy start of your own chain

#### Missing Features of Console miner

- Multithreading not used right now
- Multiple Chain mining not possible right now

___________________________________________________

### TCWallet

The TangleChain Wallet is out now! Its a really crappy one, but proves that you can use the IXI Library to write your own Wallet pretty quickly.
I had no idea how to write WPF applications, but I got it working in ~ 3 days.

___________________________________________________

### Other

There are lots of other projects planned: A Block explorer and more.

___________________________________________________


### Getting Started with IXI

The best way to get started is to check the unit tests of the IXI Library. Checkout  TestInits/Initalizing.cs  & Scnerario01.cs
There is a NuGet package, just search for "TangleChainIXI".


___________________________________________________


### Getting Started with the Console ConsoleMiner

The console miner needs an init.json file where it takes all the settings from. Just run *ConsoleMiner.exe init* and an init.json file is created in the same folder.
If you want to create a new chain, use *ConsoleMiner.exe genesis* and follow the instructions. Publish your chainsettings (eg: chainname, hash, address) and others can mine your chain too when they enter the information into their init.json file.
Use *ConsoleMiner.exe run* and the program starts mining a chain based on your init.json file.

Currently you need to have a really specific setup of commands:  
first run *init* to generate the json file. Then run *addKey -priv PRIVATEKEY* to add your private key, then you can do *genesis* or *run*

___________________________________________________

### Donation

If you want to support this project with some iotas, below is the donation address (but this is not needed tbh). The money is most likely used to pay for servers. Thank you for reading.  

XUAKJUNNUOUXMRBLDEDHNLCPBCQGVXCSPUTHMLJJTMGHOCBYAYVTQROYMFPMMOLMT9JAHARFCLKKWWBX9MNBHW9NRD
