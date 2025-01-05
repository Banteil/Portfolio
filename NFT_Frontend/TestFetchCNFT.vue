<template>
  <q-page class="q-pa-lg">
    <div class="row justify-center">
      <div class="col-8">
        <label class="text-white">Enter Merkle Tree ID</label>
        <q-input
          v-model="merkleTreeId"
          outlined
          class="custom-input"
          input-class="text-white"
          placeholder="Enter Merkle Tree ID"
        />
      </div>
      <div class="col-8 q-mt-md">
        <label class="text-white">Enter Leaf Index</label>
        <q-input
          v-model.number="leafIndex"
          outlined
          type="number"
          class="custom-input"
          input-class="text-white"
          placeholder="Enter Leaf Index"
        />
      </div>
      <div class="col-8 text-center q-mt-md">
        <q-btn
          color="primary"
          text-color="black"
          label="Fetch NFT Info"
          @click="fetchNftInfo"
        />
      </div>
    </div>

    <div v-if="nftInfo" class="q-mt-lg text-white">
      <h3>NFT Details:</h3>
      <p><strong>Mint Address:</strong> {{ nftInfo.mintAddress }}</p>
      <p><strong>Owner:</strong> {{ nftInfo.owner }}</p>
      <p><strong>Metadata URI:</strong> {{ nftInfo.metadataUri }}</p>
    </div>

    <div v-if="errorMessage" class="q-mt-lg text-red">
      <p>{{ errorMessage }}</p>
    </div>
  </q-page>
</template>

<script>
import { publicKey } from '@metaplex-foundation/umi';
import { createUmi } from '@metaplex-foundation/umi-bundle-defaults';
import { mplBubblegum, findLeafAssetIdPda } from '@metaplex-foundation/mpl-bubblegum';

const API_URL = "https://mainnet.helius-rpc.com/?api-key=c6c55bb0-4d83-49f3-8858-eefd719188ee"; // Replace with your RPC URL

export default {
  name: "FetchNftInfo",
  data() {
    return {
      merkleTreeId: '',
      leafIndex: null,
      nftInfo: null,
      errorMessage: ''
    };
  },
  methods: {
    async fetchNftInfo() {
      this.errorMessage = '';
      this.nftInfo = null;

      if (!this.merkleTreeId || this.leafIndex === null) {
        this.errorMessage = "Please enter both Merkle Tree ID and Leaf Index.";
        return;
      }

      try {
        const umi = createUmi(API_URL).use(mplBubblegum());
        const treePublicKey = publicKey(this.merkleTreeId);

        const [assetId] = await findLeafAssetIdPda(umi, {
          merkleTree: treePublicKey,
          leafIndex: this.leafIndex
        });

        const asset = await umi.rpc.getAsset(assetId);
        this.nftInfo = {
          mintAddress: asset.id.toString(),
          owner: asset.ownership.owner.toString(),
          metadataUri: asset.content.json_uri
        };
      } catch (error) {
        this.errorMessage = `Error fetching NFT info: ${error.message}`;
        console.error("Error fetching NFT info:", error);
      }
    }
  }
};
</script>

<style scoped>
.text-red {
  color: red;
}
.text-white {
  color: white;
}
</style>
