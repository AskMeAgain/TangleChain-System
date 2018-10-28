# TangleChain-System

The TangleChain-System is a collection of tools/programs to host a blockchain on top of **Iota**, each written in **C#**.

___________________________________________________


### TangleChain-IXI Library v1.2

TangleChain-IXI is the heart of the TangleChain-System. It is a **.Net Standard** Library to interact with the TangleChain-Blockchain on top of the [IOTA Tangle](https://github.com/iotaledger). It is planned to have Smartcontracts and interoperability between these chains.

#### IXI v1.2 Features:

- Reworked the Database system and included a DBManager. You can only access DB stuff from the new Manager
- Complete on Tangle blockchain
- Local database storage using **SQLite**
- Dynamic Difficulty adjustment as specified in the genesis Block
- Account System (public private key using **Nethereum**)
- Proof-of-Work Consensus in a transition phase
- NuGet Package (TangleChainIXI)

#### IXI Missing Features:

- Smartcontracts
- Interoperability
- Detect longest Chain via Blockweight
- Environment friendly consensus protocol
- Complete Documentation
- Whitepaper
- Possibility to provide own DB implementation
- Write an IOTA-Node connector for faster syncing

___________________________________________________

### Console Miner v1

This Miner is a console application which is designed to run on a server. It is currently in Beta.

#### Features of Console miner

- Easy start of your own chain

#### Missing Features of Console miner

- Multithreading not used right now
- Multiple Chain mining not possible right now
- A Docker image for even easier mining.

___________________________________________________

### TCWallet

I hardcoded lots of stuff in this to test smartcontracts in a test environment. Its not useable right now

___________________________________________________

### Other

There are lots of other projects planned:

- A Block explorer
- A programming language called Strain to use the Smartcontracts
- A Codeeditor for Strain
- A Really cool wallet

___________________________________________________

### IXI Roadmap

#### Phase 1: Bitcoin

- ~~Finish ConsoleMiner v1~~
- ~~Create Basic GUIWallet~~
- ~~difficulty adjustment~~
- ~~IXI to bitcoin functionality~~

#### Phase2: Ethereum

- Smartcontracts (50%)
- Interoperability (0%)
- Better Consensus (0%)

#### Phase3: IOTA

- Snapshot transition / ignore snapshot

___________________________________________________


### Getting Started with IXI

The best way to get started is to check the unit tests of the IXI Library. Checkout  TestInits/Initalizing.cs  & Scnerario01.cs
There is a NuGet package, just search for "TangleChainIXI".


___________________________________________________


### Getting Started with the Console ConsoleMiner

The console miner just looks into appsettings.json where it takes all the settings from.
If you want to create a new chain, use *ConsoleMiner.exe genesis* and follow the instructions. Publish your chainsettings (eg: chainname, hash, address) and others can mine your chain too when they enter the information into their appsettings.json file.
Use *ConsoleMiner.exe* and the program starts mining a chain based on your appsettings.json file.

___________________________________________________

### Donation

If you want to support this project with some iotas, below is the donation address (but this is not needed tbh). The money is most likely used to pay for servers. Thank you for reading.  

XUAKJUNNUOUXMRBLDEDHNLCPBCQGVXCSPUTHMLJJTMGHOCBYAYVTQROYMFPMMOLMT9JAHARFCLKKWWBX9MNBHW9NRD
