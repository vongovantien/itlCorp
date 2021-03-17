import { NgModule } from "@angular/core";
import { LAZY_MODULES_MAP, ILazyModules } from "src/app/load-module-map";

export const lazyModulesFClMap: ILazyModules = {
    AirExport_cdNote: () => import('./detail-job/cd-note/air-export-cd-note.module').then(m => m.AirExportCDNoteModule),
    AirExport_assignment: () => import('./detail-job/assignment/air-export-assigment.module').then(m => m.AirExportAssignmentModule),
    AirExport_attachFile: () => import('./detail-job/attach-file/air-export-attach-file.module').then(m => m.AirExportAttachFilesModule),
    AirExport_advanceSettle: () => import('./detail-job/advance-settle/air-export-advance-settle.module').then(m => m.AirExportAdvanceSettleModule)
};

@NgModule({
    providers: [
        {
            provide: LAZY_MODULES_MAP,
            useFactory: () => lazyModulesFClMap
        }
    ]
})
export class AirExportLazyLoadModule { }
