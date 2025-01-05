<template>
  <q-page class="q-pa-md page-1200 q-page-mini">
    <div class="row justify-center">
      <div class="col-12 doc-heading doc-h2 text-white">
        Test Template
      </div>
    </div>
    <div class="row justify-center q-pt-sm q-pb-sm doc-sub-heading">
      <q-btn @click="testStart" label="Start" color="green" no-caps style="width: 100%;" />
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
      signedMessage: null
    }
  },
  methods: {
    testStart() {
      console.log('■■■■■■■ Test ■■■■■■■ START')
      console.log(777)
      console.log('■■■■■■■ Test ■■■■■■■ END')
    },
    
  }
});
</script>

<style scoped>
</style>
