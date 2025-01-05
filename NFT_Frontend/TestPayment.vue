<template>
  <q-page class="q-pa-md page-1200 q-page-mini">
    <div class="row justify-center">
      <div class="col-12 doc-heading doc-h2 text-white">
        Test Payment
      </div>
    </div>

    <div class="row justify-center q-pt-sm q-pb-sm doc-sub-heading">
      <q-btn @click="testStart" label="insert payment" color="green" no-caps style="width: 100%;" />
    </div>

    <br />

    <div class="text-white">

      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">project_seq<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="project_seq" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">wallet_address_from<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="wallet_address_from" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">wallet_type_from<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="wallet_type_from" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">wallet_address_to<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="wallet_address_to" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">wallet_type_to<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="wallet_type_to" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">currency_symbol<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="currency_symbol" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">currency_amount_total<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="currency_amount_total" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">quantity_a<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="quantity_a" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">quantity_s<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="quantity_s" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">quantity_r<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="quantity_r" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">quantity_v<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="quantity_v" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">price_a<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="price_a" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">price_s<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="price_s" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">price_r<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="price_r" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">price_v<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="price_v" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">subtotal_a<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="subtotal_a" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">subtotal_s<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="subtotal_s" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">subtotal_r<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="subtotal_r" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">subtotal_v<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="subtotal_v" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">tx_id<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="tx_id" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">reg_id<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="reg_id" clearable />
        </div>
      </div>

    </div>

    <div class="row justify-center q-pt-sm q-pb-sm doc-sub-heading">
      <q-btn @click="testStart" label="insert payment" color="green" no-caps style="width: 100%;" />
    </div>

    <!-- 하단 공간 확보 -->
    <div class="row justify-center q-pa-xl">
    </div>
  </q-page>
</template>

<script>
import { defineComponent } from 'vue';

export default defineComponent({
  name: 'TestTemplate',
  data() {
    return {
      project_seq: '1',
      wallet_address_from: 'wallet_address_from',
      wallet_type_from: 'wallet_type_from',
      wallet_address_to: 'wallet_address_to',
      wallet_type_to: 'wallet_type_to',
      currency_symbol: 'currency_symbol',
      currency_amount_total: '7777777',
      quantity_a: '1',
      quantity_s: '2',
      quantity_r: '3',
      quantity_v: '4',
      price_a: '11',
      price_s: '22',
      price_r: '33',
      price_v: '44',
      subtotal_a: '111',
      subtotal_s: '222',
      subtotal_r: '333',
      subtotal_v: '444',
      tx_id: 'tx_id',
      reg_id: 'reg_id',
    }
  },
  computed: {},
  methods: {
    async testStart() {
      console.log('■■■■■■■ Test ■■■■■■■ START')


      /////////////////////////////////////////////////////////////////
      // payment 테이블에 결제정보 등록
      /////////////////////////////////////////////////////////////////
      const paramPayment = {
        project_seq: this.project_seq,
        wallet_address_from: this.wallet_address_from,
        wallet_type_from: this.wallet_type_from,
        wallet_address_to: this.wallet_address_to,
        wallet_type_to: this.wallet_type_to,
        currency_symbol: this.currency_symbol,
        currency_amount_total: this.currency_amount_total,
        quantity_a: this.quantity_a,
        quantity_s: this.quantity_s,
        quantity_r: this.quantity_r,
        quantity_v: this.quantity_v,
        price_a: this.price_a,
        price_s: this.price_s,
        price_r: this.price_r,
        price_v: this.price_v,
        subtotal_a: this.subtotal_a,
        subtotal_s: this.subtotal_s,
        subtotal_r: this.subtotal_r,
        subtotal_v: this.subtotal_v,
        tx_id: this.tx_id,
        reg_id: this.reg_id,
      }
      const resultPayment = await this.$axios.post('/api/payment/insertPayment', paramPayment)
      // console.log(result.data)
      if (!resultPayment || !resultPayment.data || resultPayment.data.returnCd != 0) {
        this.$noti(this.$q, this.$t('process_failed') + ': insertPayment')
      }

      this.$noti(this.$q, this.$t('register_success') + ' resultPayment.data.seq: ' + resultPayment.data.returnValue.seq)

      console.log('■■■■■■■ Test ■■■■■■■ END')
    },
    
  }
});
</script>

<style scoped>
</style>
