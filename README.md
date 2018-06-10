# TangleChain-System

The TangleChain-System is a collection of tools/programs to host a blockchain on top of Iota, each writting in **C#**.

#### TangleChain-IXI Library

TangleChain-IXI is the heart of the TangleChain-System. It is a **.Net Standard** Library to interact with different blockchains on top of the [IOTA Tangle](https://github.com/iotaledger). It is planned to have Smartcontracts and interoperability between these chains. The planned blocktime is around 1-2 Minutes.

#### Miner

The Miner is an example program using the Library. With this program you are able to create and mine a chain.

#### Other

There are lots of other projects planned: A Block explorer, Command line miner, a Wallet and more.

## Current Features of IXI Library

- Complete on Tangle blockchain
- Local database storage using **SQLite**
- Longest chain detection
- Easy start of a chain
- Proof-of-Work Consensus in a transition phase
- NuGet Package for easy installation.

## Missing Features

- Difficulty System to ensure blocktime of 1-2 minutes
- No Account System (public private key)
- Interoperability between different chains
- Smart contracts
- Environment friendly consensus protocol
- Complete Documentation


## Getting Started

The best way to get started is to check the unit tests of the IXI Library. Checkout the example classes in the Example Folder for a general idea.

#### Settings

Most of the functions need some settings to work. For example when Finalizing a Block/Transaction you need to have set your public key.

#### How to Mine a Block

- Create a new Block
- Fill block with:
  - Transaction hashes via AddTransactions()
  - Block Height
  - Coin Name
  - Miners Public Key
- Finalize Block via Final()
- Compute ProofOfWork via ComputeProofOfWork()
- Upload Block via Core.UploadBlock()

#### How to Send a AddTransactions

- Create a new Transactionpool
- Add a TransactionFee via AddFee()
- Add Outputs via AddOutput()
- Set Identity.SendTo to the correct TransactionPoolAddress
- Set Hash & Sign the transaction via Final()
- upload Transaction via Core.UploadTransaction();


#### Consensus

TODO, how is a block structured, TransactionPoolAddress, when is a block invalid etc.
