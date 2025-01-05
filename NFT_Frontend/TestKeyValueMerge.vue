<template>
  <q-page class="q-pa-md page-1200 q-page-mini">
    <div class="row justify-center">
      <div class="col-12 doc-heading doc-h2 text-white">
        Test KeyValueMerge
      </div>
    </div>

    <div class="row justify-center q-pt-sm q-pb-sm doc-sub-heading">
      <q-btn @click="testStart" label="merge key_value" color="green" no-caps style="width: 100%;" />
    </div>

    <br />

    <div class="text-white">

      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">cd_key<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="cd_key" clearable />
        </div>
      </div>
      
      <div class="row justify-center q-pb-sm">
        <div class="col-12">
          <span class="text-weight-bold text-subtitle1">cd_value<span class="text-red"> *</span></span>
        </div>
      </div>
      <div class="row justify-center q-pb-md">
        <div class="col-12">
          <q-input v-model="cd_value" clearable />
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
      <q-btn @click="testStart" label="merge key_value" color="green" no-caps style="width: 100%;" />
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
      cd_key: 'last_wtec_price',
      cd_value: '0.5001',
      reg_id: 'reg_id',
    }
  },
  computed: {},
  methods: {
    async testStart() {
      console.log('■■■■■■■ Test ■■■■■■■ START')


      /////////////////////////////////////////////////////////////////
      // key_value 테이블에 결제정보 등록
      /////////////////////////////////////////////////////////////////
      const paramKeyValue = {
        cd_key: this.cd_key,
        cd_value: this.cd_value,
        reg_id: this.reg_id,
      }
      const resultKeyValue = await this.$axios.post('/api/common/mergeKeyValue', paramKeyValue)
      // console.log(result.data)
      if (!resultKeyValue || !resultKeyValue.data || resultKeyValue.data.returnCd != 0) {
        this.$noti(this.$q, this.$t('process_failed') + ': insertKeyValue')
      }

      // this.$noti(this.$q, this.$t('register_success') + ' resultKeyValue.data.seq: ' + resultKeyValue.data.returnValue.seq)
      this.$noti(this.$q, this.$t('register_success'))

      console.log('■■■■■■■ Test ■■■■■■■ END')
    },
    
  }
});
</script>

<style scoped>
</style>
