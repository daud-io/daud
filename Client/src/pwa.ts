import { updateSW, registerSW } from 'virtual:pwa-register';

const updateSW = registerSW({
  onNeedRefresh() {
    updateSW();
  },
  onOfflineReady() {
    console.log("PWA offline Ready");
  },
})
