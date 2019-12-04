import { NgModule } from "@angular/core";
import { LAZY_MODULES_MAP, ILazyModules } from "src/app/load-module-map";

export const lazyModulesFClMap: ILazyModules = {
    SeaLCLImport_cdNote: () => import('./detail-job/cd-note/sea-lcl-import-cd-note.module').then(m => m.SeaLCLImportCDNoteModule),
};

@NgModule({
    providers: [
        {
            provide: LAZY_MODULES_MAP,
            useFactory: () => lazyModulesFClMap
        }
    ]
})
export class SeaLCLImportLazyLoadModule { }
