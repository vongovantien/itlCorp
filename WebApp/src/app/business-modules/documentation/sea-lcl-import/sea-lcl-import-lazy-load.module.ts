import { NgModule } from "@angular/core";
import { LAZY_MODULES_MAP, ILazyModules } from "src/app/load-module-map";

export const lazyModulesFClMap: ILazyModules = {
    SeaLCLImport_cdNote: () => import('./detail-job/cd-note/sea-lcl-import-cd-note.module').then(m => m.SeaLCLImportCDNoteModule),
    SeaLCLImport_assignment: () => import('./detail-job/assignment/sea-lcl-import-asignment.module').then(m => m.SeaLCLImportAsignmentModule),
    SeaLCLImport_advanceSettle: () => import('./detail-job/advance-settle/sea-lcl-import-advance-settle.module').then(m => m.SeaLCLImportAdvanceSettleModule)
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
