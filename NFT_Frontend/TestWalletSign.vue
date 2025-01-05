<template>
  <q-page class="q-pa-md page-1200 q-page-mini">
    <div class="row justify-center">
      <div class="col-12 doc-heading doc-h2 text-white">
        Sign Test
      </div>
    </div>
    <div class="row justify-center q-pt-sm q-pb-sm doc-sub-heading">
      <!-- <div class="col-12">
        {{ $t('menu_project_register_description') }}
      </div> -->
    </div>
    <div class="row justify-center q-pb-md">
    </div>

    <!-- <div class="row justify-center q-pt-lg">
      <div class="col-12">
        <span class="text-weight-bold text-subtitle1">{{ $t('nft_type') }}<span class="text-red"> *</span></span>
      </div>
    </div>
    <div class="row justify-center q-pb-md">
      <div class="col-12 t-text">
        <q-select
          v-model="nftType"
          :options="nftTypeOptions"
          dense
          standout
          style="width: 200px;"
          tabindex="1"
        />
      </div>
    </div>
    <div class="row justify-center q-pt-xl">
      <div class="col-12">
        <span class="text-weight-bold text-subtitle1">{{ $t('nft_id') }}<span class="text-red"> *</span></span>
      </div>
    </div>
    <div class="row justify-center q-pt-lg">
      <div class="col-12">
        <q-input
          v-model="nftId"
          ref="refNftId"
          :rules="[required, isValidNftId]"
          clearable
          standout
          tabindex="1"
        />
      </div>
    </div>
    <div class="row justify-center q-pt-xl">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">{{ $t('user_wallet_address') }}<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pt-lg">
        <div class="col-12">
          <q-input v-model="walletAddress" ref="refWalletAddress" :rules="[required, val => minLength(val, 1), val => maxLength(val, 50)]" clearable standout tabindex="1" />
        </div>
    </div> -->

    <!-- <div class="row justify-center  q-pt-lg btn-wrap"> -->
    <!-- <div class="col-6 text-left">
      <q-btn class="btn" color="grey-3" text-color="black" size="lg" style="width: 98%;" @click="goBack" tabindex="19">
        {{ $t('go_back') }}
      </q-btn>
    </div> -->
    <!-- <q-btn
      padding="xl"
      color="teal"
      round
      icon="assignment_turned_in"
      label="Sign"
      @click="signMessage"
    /> -->

    <!-- 하단 공간 확보 -->
    <div class="row justify-center q-pa-xl">
    </div>
  </q-page>
</template>

<script>
import { defineComponent } from 'vue';

export default defineComponent({
  name: 'Registration',
  data() {
    return {
      signedMessage: null
    }
  },
  methods: {
    getPhantomProvider() {
      if ('phantom' in window && window.phantom.solana && window.phantom.solana.isPhantom) {
        const provider = window.phantom.solana
        if (provider && provider.isPhantom) {
          return provider
        }
      }
      return null
    },
    async signMessage() {
      const provider = this.getPhantomProvider();
      if (!provider) {
        alert("Phantom provider not found");
        return;
      }

      try {
        const message = "To avoid digital dognappers, sign below to authenticate with CryptoCorgis";
        const encodedMessage = new TextEncoder().encode(message);
        const { signature } = await provider.signMessage(encodedMessage, "utf8");
        
        this.signedMessage = signature;
      } catch (error) {
        console.error("Failed to sign message:", error);
      }
    }
    
  }
});
</script>

<style scoped>
</style>
