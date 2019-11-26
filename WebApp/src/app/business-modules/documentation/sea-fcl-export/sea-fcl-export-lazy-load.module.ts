import { NgModule } from "@angular/core";
import { LAZY_MODULES_MAP, ILazyModules } from "src/app/load-module-map";

export const lazyModulesFClMap: ILazyModules = {
    SeaFCLExport_cdNote: () => import('./detail-job/cd-note/sea-fcl-export-cd-note.module').then(m => m.SeaFCLExportCDNoteModule),
};

@NgModule({
    providers: [
        {
            provide: LAZY_MODULES_MAP,
            useFactory: () => lazyModulesFClMap
        }
    ]
})
export class SeaFCLExportLazyLoadModule { }