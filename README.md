# TangleChain

TangleChain is a .Net Standard Library & IXI module to deploy different blockchains on top of the Tangle. It is planned to have Smartcontracts and interoperability between these chains. The planned blocktime is around 1-2 Minutes.


### Current Features

- Complete on tangle blockchain
- local database storage
- longest chain detection
- Easy start of a chain
- Proof-of-Work consensus in a transition phase
- NuGet Package for easy installation.

### Missing Features

- Difficulty System to ensure blocktime of 1-2 minutes
- No Account System (public private key)
- Interoperability between different chains
- Smart contracts
- Environment friendly consensus protocol
- Complete Documentation


## Getting Started

The best way to get started is to check the unit tests. Checkout the example classes in the Example Folder for a general idea.

#### Settings

Most of the functions need some settings to work. For example when Finalizing a Block/Transaction you need to have set your public key.

#### How to Mine a Block

- Create a new Block
- Fill block with:
  - Transaction hashes via AddTransactions()
  - Owner Public Key
  - Block Height
  - Coin Name
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
