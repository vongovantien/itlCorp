import { SeaFCLImportEffects } from './sea-fcl-import.effects';
import { ShareBussinessEffects } from 'src/app/business-modules/share-business/store/effects/share-bussiness.effect';

export * from './sea-fcl-import.effects';


export const effects: any[] = [SeaFCLImportEffects, ShareBussinessEffects];
