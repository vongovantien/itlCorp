import { NgModule } from "@angular/core";
import { LAZY_MODULES_MAP, ILazyModules } from "src/app/load-module-map";

export const lazyModulesConsolImportMap: ILazyModules = {
    SeaConsolImport_cdNote: () => import('./detail-job/cd-note/sea-consol-import-cd-note.module').then(m => m.SeaConsolImportCDNoteModule),
    SeaConsolImport_assignment: () => import('./detail-job/asignment/sea-consol-import-asignment.module').then(m => m.SeaConsolImportAsignmentModule),
    SeaConsolImport_advanceSettle: () => import('./detail-job/advance-settle/sea-consol-import-advance-settle.module').then(m => m.SeaConsolImportAdvanceSettleModule)
};

@NgModule({
    providers: [
        {
            provide: LAZY_MODULES_MAP,
            useFactory: () => lazyModulesConsolImportMap
        }
    ]
})
export class SeaConsolImportLazyLoadModule { }
