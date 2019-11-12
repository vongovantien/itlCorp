import { NgModule } from "@angular/core";
import { LAZY_MODULES_MAP, ILazyModules } from "src/app/load-module-map";

export const lazyModulesFClMap: ILazyModules = {
    cd: () => import('./detail-job/cd-note/sea-fcl-import-cd-note.module').then(m => m.SeaFCLImportCDNoteModule),
};

@NgModule({
    providers: [
        {
            provide: LAZY_MODULES_MAP,
            useFactory: () => lazyModulesFClMap
        }
    ]
})
export class SeaFCLImportLazyLoadModule { }
