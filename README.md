# TangleChain-System

The TangleChain-System is a collection of tools/programs to host a blockchain on top of [**Iota**](https://www.iota.org/), each written in **C#**.

**Project is currently on hold**

___________________________________________________


## Current Features of TangleChain-System

- Complete on Tangle blockchain
- NuGet Package ([TangleChainIXI](https://www.nuget.org/packages/TangleChainIXI/) & [Strain](https://www.nuget.org/packages/Strain/))
- Proof-of-Work Consensus in a transition phase
- Account System (public private key using **Nethereum**)
- Dynamic Difficulty adjustment as specified in the genesis Block
- Local database storage using **SQLite**
- Smartcontracts ([docs](https://github.com/AskMeAgain/TangleChain-System/tree/master/TangleChainIXI/Smartcontracts))
- Smartcontract Programming Language ([Strain](https://github.com/AskMeAgain/TangleChain-System/tree/master/Strain))

___________________________________________________

### TangleChain-IXI Library v1.3

TangleChain-IXI is the heart of the TangleChain-System. It is a **.Net Standard** Library to interact with the TangleChain-Blockchain on top of the [IOTA Tangle](https://github.com/iotaledger).

##### IXI Planned Features:

- Interoperability between Chains
- Detect longest Chain via Blockweight (right now just longest chain)
- Environment friendly consensus protocol
- ~~90% Unit test Coverage
- ~~Possibility to provide own DB implementation~~

##### IXI Roadmap

###### Phase 1: Bitcoin

- ~~Finish ConsoleMiner v1~~
- ~~Create Basic GUIWallet~~
- ~~difficulty adjustment~~
- ~~Get IXI to bitcoin functionality~~

###### Phase2: Ethereum

- ~~Smartcontracts~~
- ~~Smartcontract Language~~
- ~~Refactor of IXI~~
- Interoperability (0%)
- Better Consensus (0%)

###### Phase3: IOTA

- Snapshot transition / ignore snapshot

___________________________________________________

### Console Miner v1

This Miner is a console application which is designed to run on a server. It is currently in Alpha.

##### Features of Console miner

- Easy start of your own chain

##### Planned Features of Console miner

- Multithreading not used right now
- Multiple Chain mining not possible right now
- A Docker image for easier Setup
- Better Architecture

___________________________________________________

### TCWallet

Not implemented right now

___________________________________________________

## Getting Started

If you want to get started with TangleChain, start reading the Docs in each project. Best is to check out the integration tests.

___________________________________________________

### Donation

If you want to support this project with some iotas, below is the donation address (but this is not needed tbh). The money is most likely used to pay for servers.

Thank you for reading.  

    XUAKJUNNUOUXMRBLDEDHNLCPBCQGVXCSPUTHMLJJTMGHOCBYAYVTQROYMFPMMOLMT9JAHARFCLKKWWBX9MNBHW9NRD
