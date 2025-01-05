<template>
  <q-page class="page-1200 project-list-wrap">
    <div class="row q-pt-md q-pl-md justify-center page-tit">
      <div class="col-12 doc-heading doc-h2">
        TEST BUBBLE GUM
      </div>
    </div>
    <div class="row q-pl-md row justify-center page-sub-tit">
      <div class="col-12">
        {{ $t("test..") }}
      </div>
    </div>

    <!-- 솔라나 주소 입력 및 유효성 검사 -->
    <div class="row justify-center q-pt-lg">
      <div class="col-8 text-white">
        <label>{{ $t("Enter Solana address") }}</label>
        <q-input
          v-model="solanaAddress"
          outlined
        />
      </div>
      <div class="col-4">
        <q-btn
          class="btn"
          color="primary"
          text-color="black"
          size="lg"
          @click="checkAddress()"
        >
          {{ $t("Check Address") }}
        </q-btn>
      </div>
    </div>
    <div class="row justify-center q-pt-md">
      <div v-if="!isSolanaAddressValid" class="text-red">
        Invalid Solana address
      </div>
      <div v-if="isSolanaAddressValid" class="text-green">
        Valid Solana address
      </div>
    </div>

    <!-- 하단 공간 확보 -->
    <div class="row justify-center q-pa-xl"></div>

    <!-- 버블검 트리 생성(컬렉션 당 1건) -->
    <div class="row justify-center q-pt-lg">
      <div class="col-8 text-white">
        <label>TREE ADDRESS</label>
        <q-input
          v-model="bubbleGumTreeAddress"
          outlined
          :readonly="true"
        />
      </div>
      <div class="col-4">
        <q-btn
          class="btn"
          color="primary"
          text-color="black"
          size="lg"
          @click="createBubblegumTree()"
        >
          CREATE TREE
        </q-btn>
      </div>
    </div>
    <div v-if="isEthereumAddressValid" class="text-green">
      * 한 컬렉션 당 하나, 만든 다음 저장해 놓기
    </div>

    
    <!-- 하단 공간 확보 -->
    <div class="row justify-center q-pa-xl"></div>


    <!-- 버블검 트리 정보 가져오기 -->
    <div class="row justify-center q-pt-lg">
      <div class="col-8 text-white">
        <label>Delegate ADDRESS!!!!</label>
        <q-input
          v-model="keywordDelegateAddress"
          outlined
        />
      </div>
      <div class="col-4">
        <q-btn
          class="btn"
          color="black"
          text-color="white"
          size="lg"
          @click="setTreeDelegate()"
        >
          SET TREE DELEGATE
        </q-btn>
      </div>
    </div>

    <!-- 하단 공간 확보 -->
    <div class="row justify-center q-pa-xl"></div>

    <!-- 컬렉션 burn -->
    <div class="row justify-center q-pt-lg">
      <div class="col-8 text-white">
        <label>NFT OR COLLECTION ADDRESS</label>
        <q-input
          v-model="keywordMintAddress"
          outlined
        />
      </div>
      <div class="col-4">
        <q-btn
          class="btn"
          color="red"
          text-color="black"
          size="lg"
          @click="burnSolanaCollection()"
        >
          BURN!!
        </q-btn>
      </div>
    </div>

    <!-- 하단 공간 확보 -->
    <div class="row justify-center q-pa-xl"></div>

    <!-- 컬렉션 데이터 입력 -->
    <div class="row justify-center q-pt-lg">
      <div class="col-12 text-white">
        <h3>Collection Data</h3>
      </div>
    </div>
    <div class="row justify-center q-pt-lg">
      <div class="col-6 text-white">
        <label>{{ $t("Collection Name") }}</label>
        <q-input
          v-model="collectionData.name"
          outlined
        />
      </div>
      <div class="col-6 text-white">
        <label>{{ $t("Collection Symbol") }}</label>
        <q-input
          v-model="collectionData.symbol"
          outlined
        />
      </div>
    </div>
    <div class="row justify-center q-pt-md">
      <div class="col-12 text-white">
        <label>{{ $t("Collection URI") }}</label>
        <q-input
          v-model="collectionData.uri"
          outlined
        />
      </div>
    </div>
    <div class="row justify-center q-pt-lg">
      <div class="col-6 text-left">
        <q-btn
          class="btn"
          color="secondary"
          text-color="black"
          size="lg"
          style="width: 98%"
          @click="mintSolanaCollection()"
        >
          {{ $t("Mint Solana Collection") }}
        </q-btn>
      </div>
    </div>

    <!-- 하단 공간 확보 -->
    <div class="row justify-center q-pa-xl"></div>

    <!-- 민팅 데이터 입력 mintToCollectionV1 ver -->
    <div class="row justify-center q-pt-lg">
      <div class="col-12 text-white">
        <h3>Minting Data mintToCollectionV1 ver</h3>
      </div>
    </div>
    <div class="row justify-center q-pt-lg">
      <div class="col-6 text-white">
        <label>{{ $t("NFT Name") }}</label>
        <q-input
          v-model="nftData.name"
          outlined
        />
      </div>
      <div class="col-6 text-white">
        <label>{{ $t("NFT Symbol") }}</label>
        <q-input
          v-model="nftData.symbol"
          outlined
        />
      </div>
    </div>
    <div class="row justify-center q-pt-md">
      <div class="col-6 text-white">
        <label>{{ $t("NFT URI") }}</label>
        <q-input
          v-model="nftData.uri"
          outlined
        />
      </div>
      <div class="col-6 text-white">
        <label>{{ $t("Seller Fee Basis Points") }}</label>
        <q-input
          v-model="nftData.sellerFeeBasisPoints"
          type="number"
          outlined
        />
      </div>
    </div>
    <div class="row justify-center q-pt-md">
      <div class="col-6 text-white">
        <label>{{ $t("Token Owner Address") }}</label>
        <q-input
          v-model="nftData.tokenOwner"
          outlined
        />
      </div>
      <div class="col-6 text-white">
        <label>{{ $t("Collection Mint Address") }}</label>
        <q-input
          v-model="nftData.collectionMint"
          outlined
        />
      </div>
    </div>
    <div class="row justify-center q-pt-lg">
      <div class="col-6 text-left">
        <q-btn
          class="btn"
          color="primary"
          text-color="black"
          size="lg"
          style="width: 98%"
          @click="mintSolanaNftBubblegum()"
        >
          {{ $t("Mint Solana NFT") }}
        </q-btn>
      </div>
    </div>

    <!-- 하단 공간 확보 -->
    <div class="row justify-center q-pa-xl"></div>

    <!-- 민팅 데이터 입력 MintV1 ver -->
    <div class="row justify-center q-pt-lg">
      <div class="col-12 text-white">
        <h3>Minting Data MintV1 ver</h3>
      </div>
    </div>
    <div class="row justify-center q-pt-lg">
      <div class="col-6 text-white">
        <label>{{ $t("NFT Name") }}</label>
        <q-input
          v-model="nftData.name"
          outlined
        />
      </div>
      <div class="col-6 text-white">
        <label>{{ $t("NFT Symbol") }}</label>
        <q-input
          v-model="nftData.symbol"
          outlined
        />
      </div>
    </div>
    <div class="row justify-center q-pt-md">
      <div class="col-6 text-white">
        <label>{{ $t("NFT URI") }}</label>
        <q-input
          v-model="nftData.uri"
          outlined
        />
      </div>
      <div class="col-6 text-white">
        <label>{{ $t("Seller Fee Basis Points") }}</label>
        <q-input
          v-model="nftData.sellerFeeBasisPoints"
          type="number"
          outlined
        />
      </div>
    </div>
    <div class="row justify-center q-pt-md">
      <div class="col-6 text-white">
        <label>{{ $t("Token Owner Address") }}</label>
        <q-input
          v-model="nftData.tokenOwner"
          outlined
        />
      </div>
      <div class="col-6 text-white">
        <label>{{ $t("Collection Mint Address") }}</label>
        <q-input
          v-model="nftData.collectionMint"
          outlined
        />
      </div>
    </div>
    <div class="row justify-center q-pt-lg">
      <div class="col-6 text-left">
        <q-btn
          class="btn"
          color="primary"
          text-color="black"
          size="lg"
          style="width: 98%"
          @click="mintSolanaNftBubblegumMintV1()"
        >
          {{ $t("Mint Solana NFT") }}
        </q-btn>
      </div>
    </div>

    <!-- 하단 공간 확보 -->
    <div class="row justify-center q-pa-xl"></div>

    <!-- 메타데이터 업데이트 데이터 입력 -->
    <div class="row justify-center q-pt-lg">
      <div class="col-12 text-white">
        <h3>Update CNFT Metadata</h3>
      </div>
    </div>

    <!-- Asset ID 입력 -->
    <div class="row justify-center q-pt-md">
      <div class="col-8 text-white">
        <label>Asset ID (Mint Address)</label>
        <q-input v-model="updateData.assetId" outlined />
      </div>
    </div>

    <!-- 메타데이터 이름 입력 -->
    <div class="row justify-center q-pt-md">
      <div class="col-8 text-white">
        <label>New NFT Name</label>
        <q-input v-model="updateData.name" outlined />
      </div>
    </div>

    <!-- 메타데이터 URI 입력 -->
    <div class="row justify-center q-pt-md">
      <div class="col-8 text-white">
        <label>New Metadata URI</label>
        <q-input v-model="updateData.uri" outlined />
      </div>
    </div>

    <!-- 메타데이터 업데이트 버튼 -->
    <div class="row justify-center q-pt-lg">
      <div class="col-6 text-left">
        <q-btn
          class="btn"
          color="warning"
          text-color="black"
          size="lg"
          style="width: 98%"
          @click="updateBubblegumMetadata()"
        >
          {{ $t("Update CNFT Metadata") }}
        </q-btn>
      </div>
    </div>


    <!-- 하단 공간 확보 -->
    <div class="row justify-center q-pa-xl"></div>

    <!-- 에어드랍 기능 추가 -->
    <div class="row justify-center q-pt-lg">
      <div class="col-12 text-white">
        <h3>Transfer Solana NFT</h3>
      </div>
    </div>
    <div class="row justify-center q-pt-lg">
      <div class="col-6 text-white">
        <label>{{ $t("Recipient Wallet Address") }}</label>
        <q-input
          v-model="solanaTransferAddress"
          outlined
        />
      </div>
      <div class="col-6 text-white">
        <label>{{ $t("NFT Mint Address to Transfer") }}</label>
        <q-input
          v-model="solanaTransferMintAddress"
          outlined
        />
      </div>
      <div class="col-6 text-left">
        <q-btn
          class="btn"
          color="warning"
          text-color="black"
          size="lg"
          style="width: 98%"
          @click="transferSolanaNft()"
        >
          {{ $t("Transfer Solana NFT") }}
        </q-btn>
      </div>
    </div>

    <!-- 하단 공간 확보 -->
    <div class="row justify-center q-pa-xl"></div>

    <!-- 이더리움 주소 입력 및 유효성 검사 -->
    <div class="row justify-center q-pt-lg">
      <div class="col-8 text-white">
        <label>{{ $t("Enter Ethereum address") }}</label>
        <q-input
          v-model="ethereumAddress"
          outlined
        />
      </div>
      <div class="col-4">
        <q-btn
          class="btn"
          color="primary"
          text-color="black"
          size="lg"
          @click="checkEthereumAddress()"
        >
          {{ $t("Check Address") }}
        </q-btn>
      </div>
    </div>
    <div class="row justify-center q-pt-md">
      <div v-if="!isEthereumAddressValid" class="text-red">
        Invalid Ethereum address
      </div>
      <div v-if="isEthereumAddressValid" class="text-green">
        Valid Ethereum address
      </div>
    </div>

    <!-- 하단 공간 확보 -->
    <div class="row justify-center q-pa-xl"></div>
    
    <!-- Wallet Address Input -->
    <div class="row justify-center q-pt-lg">
      <div class="col-8 text-white">
        <label>{{ $t("Enter Wallet Address") }}</label>
        <q-input
          v-model="walletAddress"
          outlined
        />
      </div>
      <div class="col-4">
        <q-btn
          class="btn"
          color="primary"
          text-color="black"
          size="lg"
          @click="fetchEthereumNftList()"
        >
          {{ $t("Fetch NFTs") }}
        </q-btn>
      </div>
      <div class="text-green">
        Get nft list
      </div>
    </div>
    <div class="text-white q-pa-xl">
      <h1>사용자가 소유한 NFT 목록</h1>
      <div>
        <input type="checkbox" v-model="selectAll" @change="toggleSelectAll" /> 전체 선택 / 취소
      </div>
      <div v-if="ownNftList.length > 0">
        <div v-for="(nft, index) in ownNftList" :key="index" class="nft-item q-pa-xl">
          <input type="checkbox" v-model="nft.selected" />
          <img :src="nft.metadata.image" :alt="nft.title" class="nft-image" style="width: 100px; height: auto;"/>
          <h2>nft title: {{ nft.title }}</h2>
          <p>description: {{ nft.description }}</p>
          <p>contract name: {{ nft.contractMetadata.name }}</p>
          <p>token ID: {{ nft.id.tokenId }}</p>
          <p>balance: {{ nft.balance }}</p>
        </div>
      </div>
      <div v-else>
        <p>소유한 NFT가 없습니다.</p>
      </div>
    </div>



    
    <!-- 하단 공간 확보 -->
    <div class="row justify-center q-pa-xl"></div>
  </q-page>
</template>

<script>
import { defineComponent } from "vue";
import { useI18n } from "vue-i18n";
import { PublicKey, Keypair, Connection } from "@solana/web3.js";
import { getOrCreateAssociatedTokenAccount, transfer, TOKEN_PROGRAM_ID } from "@solana/spl-token";
import bs58 from 'bs58';
import { createUmi } from "@metaplex-foundation/umi-bundle-defaults";
import { generateSigner, keypairIdentity, percentAmount, TransactionBuilder, some, none } from "@metaplex-foundation/umi";
import { setComputeUnitLimit, setComputeUnitPrice } from '@metaplex-foundation/mpl-toolbox';
import { createNft, mplTokenMetadata, fetchDigitalAsset, collectionAuthorityRecordPda, verifyCollectionV1, findMetadataPda, burnV1, TokenStandard  } from "@metaplex-foundation/mpl-token-metadata";
import { createTree, mintToCollectionV1, parseLeafFromMintToCollectionV1Transaction, findLeafAssetIdPda, getAssetWithProof, verifyCollection, updateMetadata, mintV1, mplBubblegum, getCurrentRoot, fetchMerkleTree, setTreeDelegate   } from '@metaplex-foundation/mpl-bubblegum'
import Web3 from 'web3';

export default defineComponent({
  name: "Test",
  setup() {
    const { locale } = useI18n({ useScope: "global" });
    return {
      locale,
    };
  },
  data() {
    return {
      solanaAddress: '',
      bubbleGumTreeAddress: '1hVF5pFhMkxvvYyB49imWGqgbFXttiqmskTjnKtajip',
      keyword: '',
      keywordMintAddress: '',
      keywordDelegateAddress: '',
      solanaTransferAddress: '',
      solanaTransferMintAddress: '',
      walletAddress: '0x5f1DBe428EBAb9A9aA724aBd0Fff3d6f64E79172',
      contractAddress: '0x46FdfCb3Cd89A1C54D36EE83a4ADC184747B40D9',
      ownNftList: [],
      selectAll: false,
      isSolanaAddressValid: true,
      ethereumAddress: '',
      isEthereumAddressValid: true,
      solanaNetwork: "https://devnet.helius-rpc.com/?api-key=c6c55bb0-4d83-49f3-8858-eefd719188ee",
      //solanaNetwork: "https://quaint-thrumming-mountain.solana-mainnet.quiknode.pro/19adaf15ef3c5a0ae692d1ccec0e47244f261d2a",
      payerSecretKey: '',
      nftData: {
        name: "TEST BUBBLE",
        symbol: "TB",
        uri: "https://arweave.net/tx0uyd4lDGPvZIUMBgz2-jJEdTBMCky8kqxu06dufM4",
        sellerFeeBasisPoints: 7.5,
        tokenOwner: "4gbdX7szo3RN4nNmc8BFM6buAkfu9sKhHSbp1uo4CLUF",
        collectionMint: "5Jjtfhva68WFPXGsqDvk1DmpPT4JHkzy8S13UpuRRBqE",
      },
      updateData: {
        assetId: '',
        name: '',
        uri: '',
      },
      collectionData: {
        name: "Sand Bang X Punky Kongz Colab",
        symbol: "SKONGZ",
        uri: "https://punkykongz.com/nft/skongz/collection/skongz_collection.json",
        sellerFeeBasisPoints: 500,
      },
    };
  },
  created: function () {
    // 키 벨류 조회
    this.selectKeyValue()
  },
  methods: {
    async checkAddress() {
      this.isSolanaAddressValid = await this.isValidSolanaAddress(this.solanaAddress);
      console.log(this.isSolanaAddressValid ? 'Valid Solana address' : 'Invalid Solana address');
    },
    async checkEthereumAddress() {
      const web3 = new Web3();
      this.isEthereumAddressValid = web3.utils.isAddress(this.ethereumAddress);
      console.log(this.isEthereumAddressValid ? 'Valid Ethereum address' : 'Invalid Ethereum address');
    },
    async selectKeyValue() {
      const paramKeyValuePayerSecretKey = {
        cdKey: this.$KEY_VALUE_SOLANA_PAYER_SECRET_KEY,
      }
      const resultKeyValuePayerSecretKey = await this.$axios.get('/api/common/selectKeyValue', { params: { ...paramKeyValuePayerSecretKey }})
      // console.log(resultKeyValue.data)
      if (resultKeyValuePayerSecretKey.data) {
        this.payerSecretKey = resultKeyValuePayerSecretKey.data.returnValue
      }

      const paramKeyValueSellerFeeBasisPoint = {
        cdKey: this.$KEY_VALUE_SOLANA_SELLER_FEE_BASIS_POINT,
      }
      const resultKeyValueSellerFeeBasisPoint = await this.$axios.get('/api/common/selectKeyValue', { params: { ...paramKeyValueSellerFeeBasisPoint }})
      // console.log(resultKeyValue.data)
      if (resultKeyValueSellerFeeBasisPoint.data) {
        this.collectionData.sellerFeeBasisPoints = resultKeyValueSellerFeeBasisPoint.data.returnValue
        this.nftData.sellerFeeBasisPoints = resultKeyValueSellerFeeBasisPoint.data.returnValue
      }

      // const paramKeyValueBubblegumTreeAddress = {
      //   cdKey: this.$KEY_VALUE_BUBBLEGUM_TREE_ADDRESS,
      // };
      // const resultKeyValueBubblegumTreeAddress = await this.$axios.get('/api/common/selectKeyValue', { params: { ...paramKeyValueBubblegumTreeAddress }})
      // if (resultKeyValueBubblegumTreeAddress.data) {
      //   this.bubbleGumTreeAddress = resultKeyValueBubblegumTreeAddress.data.returnValue
      // }
    },
    // 버블검 트리 생성
    async createBubblegumTree() {
      try {
        console.log("■■■■■■■ Creating Bubblegum Tree ■■■■■■■ START");

        // 1. UMI 인스턴스 생성 및 네트워크 설정
        const umi = createUmi(this.$SOLANA_NETWORK).use(mplTokenMetadata());

        // 2. Payer 설정 (지갑 비밀키 사용)
        const payer = umi.eddsa.createKeypairFromSecretKey(bs58.decode(this.payerSecretKey));
        umi.use(keypairIdentity(payer));
        console.log("Payer account:", payer.publicKey);

        // 3. 트리 계정 생성
        const merkleTree = generateSigner(umi);
        console.log("Generated Tree account:", merkleTree.publicKey);

        // 트리 생성자가 올바르게 설정되었는지 확인
        const treeCreator = payer.publicKey;
        console.log("Tree Creator:", treeCreator);

        // 4. Bubblegum 트리 생성
        const tx = await createTree(umi, {
          merkleTree,
          maxDepth: 14,
          maxBufferSize: 64,
          // public: false,
          treeCreator: treeCreator
        });

        // 트랜잭션 전송 및 확인
        await tx.sendAndConfirm(umi);
        console.log("Tree Address:", merkleTree.publicKey);
        console.log("■■■■■■■ Bubblegum Tree Created ■■■■■■■ END");

        // 트리 주소 저장
        this.bubbleGumTreeAddress = merkleTree.publicKey;

      } catch (error) {
        console.error("Error creating Bubblegum Tree:", error);
        return null;
      }
    },
    // 버블검 트리 가져오기
    async getBubblegumTree() {
      try {
        console.log("■■■■■■■ Get Bubblegum Tree Info ■■■■■■■ START");

        // 1. UMI 인스턴스 생성 및 네트워크 설정
        const umi = createUmi(this.$SOLANA_NETWORK).use(mplTokenMetadata());

        // // 2. Payer 설정 (지갑 비밀키 사용)
        // const payer = umi.eddsa.createKeypairFromSecretKey(bs58.decode(this.payerSecretKey));
        // umi.use(keypairIdentity(payer));
        // console.log("Payer account:", payer.publicKey);

        // // 3. 트리 계정 생성
        // const merkleTree = generateSigner(umi);
        // console.log("Generated Tree account:", merkleTree.publicKey);

        const merkleTreeAccount = await fetchMerkleTree(umi, this.bubbleGumTreeAddress)

        // 트리 존재 여부 확인
        const treeExists = merkleTreeAccount?.header?.exists === true

        console.log("merkleTreeAccount:", merkleTreeAccount);
        console.log("merkleTreeAccount Exists? >>> ", treeExists);
        console.log("■■■■■■■ Get Bubblegum Tree Info ■■■■■■■ END");

      } catch (error) {
        console.error("Error creating Bubblegum Tree:", error);
        return null;
      }
    },
    // 버블검 트리 권한 위임
    async setTreeDelegate() {
      try {
        console.log("■■■■■■■ set Bubblegum Tree Delegate ■■■■■■■ START");

        // 1. UMI 인스턴스 생성 및 네트워크 설정
        const umi = createUmi(this.$SOLANA_NETWORK).use(mplTokenMetadata());

        // 2. Payer 설정 (지갑 비밀키 사용)
        const payer = umi.eddsa.createKeypairFromSecretKey(bs58.decode(this.payerSecretKey));
        umi.use(keypairIdentity(payer));
        console.log("Payer account:", payer.publicKey);

        // // 3. 트리 계정 생성
        // const merkleTree = generateSigner(umi);
        // console.log("Generated Tree account:", merkleTree.publicKey);

        const setTreeDelegateResult = await setTreeDelegate(umi, {
                                                                merkleTree: this.bubbleGumTreeAddress,
                                                                treeCreator: payer.publicKey,
                                                                newTreeDelegate: this.keywordDelegateAddress,
                                                              }).sendAndConfirm(umi)

        console.log("setTreeDelegate:", bs58.encode(new Uint8Array(setTreeDelegateResult.signature)));
        console.log("■■■■■■■ set Bubblegum Tree Delegate ■■■■■■■ END");

      } catch (error) {
        console.error("Error creating Bubblegum Tree:", error);
        return null;
      }
    },
    // mintToCollectionV1을 이용한 민팅(따로 컬렉션 검증이 필요없음)
    async mintSolanaNftBubblegum() {
      try {
        console.log("■■■■■■■ mintSolana CNFT Test START ■■■■■■■");

        // Umi 인스턴스 생성 및 설정
        const umi = createUmi(this.$SOLANA_NETWORK).use(mplTokenMetadata());
        const payer = umi.eddsa.createKeypairFromSecretKey(bs58.decode(this.payerSecretKey));
        umi.use(keypairIdentity(payer));

        // 필요한 PublicKey 생성
        const collectionMint = new PublicKey(this.nftData.collectionMint);
        const leafOwner = new PublicKey(this.nftData.tokenOwner);
        const merkleTree = new PublicKey(this.bubbleGumTreeAddress);
        const name = this.nftData.name
        const symbol = this.nftData.symbol
        const uri = this.nftData.uri

        // 트랜잭션 빌더 생성
        let builder = new TransactionBuilder()
          .add(setComputeUnitLimit(umi, { units: 500_000 }))
          .add(setComputeUnitPrice(umi, { microLamports: 10 }))
          .add(
            mintToCollectionV1(umi, {
              leafOwner,
              merkleTree,
              collectionMint,
              collectionAuthority: payer.publicKey,
              metadata: {
                name,
                symbol,
                uri,
                sellerFeeBasisPoints: 500,
                collection: {
                  key: collectionMint,
                  verified: false,
                },
                creators: [
                  {
                    address: payer.publicKey,
                    verified: true,
                    share: 100,
                  },
                ],
              },
            })
          );

        // 최신 블록 해시 설정
        builder = await builder.setLatestBlockhash(umi);

        // 트랜잭션 빌드 및 서명
        const transaction = await builder.buildAndSign(umi);

        // 트랜잭션 전송 및 확인
        const signatureArray = await umi.rpc.sendTransaction(transaction, { commitment: "finalized" })

        const signature = bs58.encode(new Uint8Array(signatureArray));

        console.log("Minted CNFT Transaction Signature:", signature);

        // 트랜잭션에서 Leaf 데이터 추출
        // CNFT의 Asset ID 가져오기
        const leaf = await this.parseLeafWithRetries(umi, signatureArray)
        const assetId = leaf.id
        console.log('CNFT Asset ID >>>>> ', assetId)
        console.log('CNFT signature >>>>> ', signature)

        console.log("CNFT Minting Completed Successfully");
        // return {
        //   mint_account_key: assetId,
        //   create_nft_signature: signature,
        // };
      } catch (error) {
        console.error("Error minting CNFT:", error);
      }
    },
    // mintV1을 통한 민팅 (에러남)
    async mintSolanaNftBubblegumMintV1() {
      try {
        console.log("■■■■■■■ mintSolana CNFT Test START ■■■■■■■");

        const DAS_URL = 'https://devnet.irys.xyz';

        // 기본 설정
        const umi = createUmi(this.$SOLANA_NETWORK).use(mplTokenMetadata());
        const payer = umi.eddsa.createKeypairFromSecretKey(bs58.decode(this.payerSecretKey));
        umi.use(keypairIdentity(payer));
        console.log("Payer account:", payer.publicKey);
        const collectionKey = new PublicKey(this.nftData.collectionMint);

        const leafOwner = new PublicKey(this.nftData.tokenOwner);
        const merkleTree = new PublicKey(this.bubbleGumTreeAddress);

        console.log("Minting CNFT...");

        // 트랜잭션 빌더 생성 및 컴퓨팅 리소스 설정 추가
        const transactionBuilder = new TransactionBuilder()
          .add(setComputeUnitLimit(umi, { units: 400_000 })) // 컴퓨팅 유닛 한도 설정
          .add(setComputeUnitPrice(umi, { microLamports: 1 })); // 우선순위 높이기 위해 수수료 설정

        // CNFT 민팅 트랜잭션 추가
        const { signature } = await transactionBuilder
          .add(
            mintV1(umi, {
              leafOwner,
              merkleTree,
              metadata: {
                name: this.nftData.name,
                uri: this.nftData.uri,
                sellerFeeBasisPoints: 500,
                collection: { key: this.nftData.collectionMint, verified: false },
                creators: [
                  {
                    address: payer.publicKey,
                    verified: false,
                    share: 100,
                      },
                    ],
                  },
                })
              )
              .sendAndConfirm(umi, { commitment: "finalized" });

        console.log("Minted CNFT Transaction Signature:", bs58.encode(signature));

        // Leaf 데이터 추출
        const leaf = await this.parseLeafWithRetries(umi, signature);
        const assetId = findLeafAssetIdPda(umi, {
          merkleTree: merkleTree,
          leafIndex: leaf.nonce,
        })
        console.log('Compressed NFT Asset ID:', assetId.toString())

        // 3. Asset 정보와 증명 데이터 가져오기
        console.log("Fetching asset with proof...");

        // Fetch the asset using umi rpc with DAS.
        const asset = await umi.rpc.getAsset(assetId[0])
        console.log({ asset })

        const assetWithProof = await getAssetWithProof(umi, assetId[0], {truncateCanopy: true});

        // 4. 컬렉션 설정 및 검증
        console.log("Setting and verifying collection...");
        const { signature: verifySignature } = await verifyCollection(umi, {
          ...assetWithProof,
          // treeCreatorOrDelegate: payer.publicKey,
          collectionMint: collectionKey,
          collectionAuthority: umi.identity,
        }).sendAndConfirm(umi, { commitment: 'finalized' });


        console.log("Verified Collection Signature:", bs58.encode(verifySignature));
        console.log("Collection successfully set and verified.");

        console.log("■■■■■■■ CNFT Minting Completed ■■■■■■■");

      } catch (error) {
        console.error("Error minting CNFT:", error);
      }
    },
    async parseLeafWithRetries(umi, signature, retries = 100, delay = 2000) {
      for (let attempt = 0; attempt < retries; attempt++) {
        try {
          const leaf = await parseLeafFromMintToCollectionV1Transaction(umi, signature);
          if (leaf) {
            return leaf;
          }
        } catch (error) {
          console.warn(`Attempt ${attempt + 1} failed:`, error);
        }
        await new Promise(resolve => setTimeout(resolve, delay)); // 대기 후 재시도
      }
      throw new Error("Failed to parse leaf after multiple attempts");
    },
    async getCNFTAssetId(umi, signature, merkleTree) {
      // 트랜잭션에서 리프 정보 추출
      const leaf = await parseLeafFromMintToCollectionV1Transaction(umi, signature);
      console.log('Leaf Info:', leaf);

      // Asset ID 조회
      const assetId = findLeafAssetIdPda(umi, {
        merkleTree,
        leafIndex: leaf.nonce
      });
      console.log('CNFT Asset ID:', assetId);

      return assetId[0]
    },
    async mintSolanaCollection() {
      try {
        console.log("■■■■■■■ mintSolanaCollection Test ■■■■■■■ START");
        const umi = createUmi(this.$SOLANA_NETWORK).use(mplTokenMetadata());
        const payer = umi.eddsa.createKeypairFromSecretKey(bs58.decode(this.payerSecretKey));
        umi.use(keypairIdentity(payer));

        const collectionMint = generateSigner(umi);
        console.log("Collection mint account : ", collectionMint.publicKey); 

        const createdCollection = await createNft(umi, {
          mint: collectionMint,
          name: this.collectionData.name,
          symbol: this.collectionData.symbol,
          uri: this.collectionData.uri,
          sellerFeeBasisPoints: percentAmount(5),
          creators: [
            {
              address: payer.publicKey,
              verified: true,
              share: 100,
            },
          ],
          authority: payer.publicKey,
          tokenOwner: payer.publicKey,
          isCollection: true,
        }).sendAndConfirm(umi);

        console.log(
          "Create Collection Signature : ",
          bs58.encode(createdCollection.signature)
        )
        console.log(
          "Create Collection mint address : ",
          createdCollection.publicKey
        )
        console.log("■■■■■■■ mintSolanaCollection Test ■■■■■■■ END");
      } catch (error) {
        console.error("Error minting Solana Collection:", error);
      }
    },
    async burnSolanaCollection() {
      try {
        console.log("■■■■■■■ Burn Solana Collection ■■■■■■■ START");

        // 1. UMI 인스턴스 생성
        const umi = createUmi(this.$SOLANA_NETWORK).use(mplTokenMetadata());

        // 2. Payer 설정 (NFT 소유자)
        const owner = umi.eddsa.createKeypairFromSecretKey(bs58.decode(this.payerSecretKey));
        umi.use(keypairIdentity(owner));

        console.log("Mint Address to burn:", this.keywordMintAddress);
        console.log("Owner Public Key:", owner.publicKey);

        // 3. NFT 삭제 트랜잭션 실행
        const burnTx = await burnV1(umi, {
          mint: this.keywordMintAddress,
          authority: owner, // Authority 계정
          tokenOwner: owner.publicKey, // NFT 소유자
          tokenStandard: TokenStandard.NonFungible, // NFT 표준
        }).sendAndConfirm(umi);

        console.log("Burn Transaction Signature:", bs58.encode(new Uint8Array(burnTx.signature)));
        console.log("■■■■■■■ Burn Solana Collection ■■■■■■■ END");
      } catch (error) {
        console.error("Error burning Solana Collection:", error);
      }
    },
    async transferSolanaNft() {
      try {
        console.log("■■■■■■■ transferSolanaNft Test ■■■■■■■ START");
        const connection = new Connection(this.$SOLANA_NETWORK, 'confirmed');
        const payer = Keypair.fromSecretKey(bs58.decode(this.payerSecretKey));
        const mintPublicKey = new PublicKey(this.solanaTransferMintAddress);
        const recipientPublicKey = new PublicKey(this.solanaTransferAddress);

        console.log("mintPublicKey : ", this.solanaTransferMintAddress);
        console.log("recipientPublicKey : ", this.solanaTransferAddress);

        const payerTokenAccount = await getOrCreateAssociatedTokenAccount(
          connection,
          payer,
          mintPublicKey,
          payer.publicKey
        );
        const recipientTokenAccount = await getOrCreateAssociatedTokenAccount(
          connection,
          payer,
          mintPublicKey,
          recipientPublicKey
        );

        // Transfer the token
        const signature = await transfer(
          connection,
          payer,
          payerTokenAccount.address,
          recipientTokenAccount.address,
          payer,
          1, // Amount to transfer (for NFTs, typically 1)
          [],
          TOKEN_PROGRAM_ID
        );

        console.log("Transfer NFT Signature : ", signature);
        console.log("■■■■■■■ transferSolanaNft Test ■■■■■■■ END");
      } catch (error) {
        console.error("Error transferring Solana NFT:", error);
      }
    },
    // async fetchEthereumNftList() {
    //   try 
    //   {
    //     const provider = new ethers.providers.JsonRpcProvider('https://rpc.ankr.com/eth');
    //     console.log('provider : ', provider);
    //     const contract = new ethers.Contract(this.contractAddress, this.$ERC721_ABI, provider);
    //     console.log('contract : ', contract);

    //     // 사용자가 소유한 NFT의 총 개수를 가져옵니다.
    //     const balance = await contract.balanceOf(this.walletAddress);
    //     console.log(`Total NFTs owned: ${balance.toString()}`);
        
    //     if (balance > 0) {
    //       let ownedTokenIds = [];
    //       // 모든 토큰 ID를 순회하며 소유자가 일치하는 토큰을 가져옵니다.
    //       const totalSupply = await contract.totalSupply();
    //       for (let tokenId = 0; tokenId < totalSupply; tokenId++) {
    //         try {
    //           const owner = await contract.ownerOf(tokenId);
    //           if (owner.toLowerCase() === this.walletAddress.toLowerCase()) {
    //             ownedTokenIds.push(tokenId.toString());
    //             console.log(`Token ID owned by ${this.walletAddress}: ${tokenId}`);
    //           }
    //         } catch (error) {
    //           console.error(`Failed to fetch owner of token ID ${tokenId}:`, error);
    //         }
    //       }
    //       console.log('Owned Token IDs:', ownedTokenIds);
    //     } else {
    //       console.log('No NFTs owned by this address');
    //     }
    //   } catch (error) {
    //     console.error('Error fetching NFT details:', error);
    //     throw error;
    //   }
    // },    
    async fetchEthereumNftList() {
      const ownerAddress = this.walletAddress
      const contractAddress = this.contractAddress
      
      const apiKey = "wspRO3sE6afxI_nRqFoKLsz6sLDielYd"; // Alchemy API 키
      const url = `https://eth-mainnet.alchemyapi.io/v2/${apiKey}/getNFTs/`;

      try {
          const response = await this.$axios.get(url, {
              params: {
                  owner: ownerAddress,
                  contractAddresses: [contractAddress]
              }
          });

          console.log("사용자가 소유한 특정 컨트랙트의 NFT:", response.data)

          this.ownNftList = response.data.ownedNfts


          console.log('this.ownNftList')
          console.log(this.ownNftList)
      } catch (error) {
          console.error("NFT 조회 중 오류 발생:", error)
      }
    },    
    async isValidSolanaAddress(address) {
      try {
        let key = new PublicKey(address);
        return PublicKey.isOnCurve(key.toBuffer())
      } catch (error) {
        console.log('error : ', error)
        return false
      }
    },
    async fetchAssetWithRetries(umi, publicKey, retries = 5, delay = 100) {
      for (let i = 0; i < retries; i++) {
        try {
          const asset = await fetchDigitalAsset(umi, publicKey)
          return asset
        } catch (error) {
          if (i < retries - 1) {
            await new Promise((resolve) => setTimeout(resolve, delay))
          }
        }
      }
      throw new Error("Failed to fetch asset after multiple attempts")
    },


    toggleSelectAll() {
      this.ownNftList.forEach(nft => {
        nft.selected = this.selectAll
      })
    },

    async updateBubblegumMetadata() {
      try {
        console.log("■■■■■■■ Updating Bubblegum CNFT Metadata ■■■■■■■ START");

        // 1. UMI 인스턴스 생성 및 네트워크 설정
        const umi = createUmi(this.solanaNetwork)
          .use(mplTokenMetadata())
          .use(mplBubblegum());

        const payer = umi.eddsa.createKeypairFromSecretKey(bs58.decode(this.payerSecretKey));
        umi.use(keypairIdentity(payer));

        const leafOwner = new PublicKey(this.nftData.tokenOwner);
        const collectionMint = new PublicKey(this.nftData.collectionMint);
        const assetId = new PublicKey(this.updateData.assetId);

        // 2. Proof 가져오기 (도우미 메서드 사용)
        const assetWithProof = await getAssetWithProof(umi, assetId, { truncateCanopy: true });
        console.log("Current Metadata :", assetWithProof.metadata);

        // 3. 업데이트할 메타데이터 설정
        const updateArgs = {
          name: some(this.updateData.name),
          uri: some(this.updateData.uri),
        };

        // 4. 메타데이터 업데이트 (도우미 메서드 사용)
        const { signature } = await updateMetadata(umi, {
          ...assetWithProof,
          leafOwner,
          currentMetadata: assetWithProof.metadata,
          updateArgs,
          authority: payer,
          collectionMint: collectionMint,
        }).sendAndConfirm(umi);

        console.log("Updated Metadata Transaction Signature:", bs58.encode(signature));

        console.log("■■■■■■■ CNFT Metadata Update Completed ■■■■■■■ END");
      } catch (error) {
        console.error("Error updating CNFT metadata:", error);
      }
    },
  },
});
</script>

<style scoped></style>