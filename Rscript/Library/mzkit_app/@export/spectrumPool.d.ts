﻿// export R# package module type define for javascript/typescript language
//
//    imports "spectrumPool" from "mzDIA";
//
// ref=mzkit.MolecularSpectrumPool@mzDIA, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null

/**
 * Spectrum clustering/inference via molecule networking method, 
 *  this api module is working with the biodeep public cloud service
 * 
*/
declare namespace spectrumPool {
   /**
    * add sample peaks data to spectrum pool
    * 
    * > the spectrum data for run clustering should be 
    * >  processed into centroid mode at first!
    * 
     * @param pool -
     * @param x the spectrum data collection
     * @param biosample -
     * 
     * + default value Is ``'unknown'``.
     * @param organism -
     * 
     * + default value Is ``'unknown'``.
     * @param project 
     * + default value Is ``'unknown'``.
     * @param instrument 
     * + default value Is ``'unknown'``.
     * @param file 
     * + default value Is ``'unknown'``.
     * @param filename_overrides 
     * + default value Is ``false``.
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function addPool(pool: object, x: any, biosample?: string, organism?: string, project?: string, instrument?: string, file?: string, filename_overrides?: boolean, env?: object): any;
   /**
    * close the connection to the spectrum pool
    * 
    * 
     * @param pool -
   */
   function closePool(pool: object): any;
   /**
    * commit data to the spectrum pool database
    * 
    * 
     * @param pool -
   */
   function commit(pool: object): any;
   /**
    * generates the guid for the spectrum with unknown annotation
    * 
    * 
     * @param spectral -
     * @param env 
     * + default value Is ``null``.
   */
   function conservedGuid(spectral: any, env?: object): string;
   /**
    * create a new spectrum clustering data pool
    * 
    * 
     * @param link -
     * @param level -
     * 
     * + default value Is ``0.9``.
     * @param split hex, max=15
     * 
     * + default value Is ``9``.
     * @param name 
     * + default value Is ``'no_named'``.
     * @param desc 
     * + default value Is ``'no_information'``.
   */
   function createPool(link: string, level?: number, split?: object, name?: string, desc?: string): object;
   /**
    * get metadata dataframe in a given cluster tree
    * 
    * 
     * @param pool -
     * @param path -
     * 
     * + default value Is ``null``.
   */
   function getClusterInfo(pool: object, path?: string): any;
   /**
    * Infer and make annotation to a specific cluster
    * 
    * 
     * @param dia -
     * @param cluster_id -
     * @param reference_id 
     * + default value Is ``null``.
     * @param formula 
     * + default value Is ``null``.
     * @param name 
     * + default value Is ``null``.
   */
   function infer(dia: object, cluster_id: string, reference_id?: string, formula?: string, name?: string): object;
   /**
    * Create a spectrum inference protocol workflow
    * 
    * 
     * @param url -
     * @param model_id -
     * @param ms1diff 
     * + default value Is ``'da:0.3'``.
     * @param ms2diff 
     * + default value Is ``'da:0.3'``.
     * @param intocutoff 
     * + default value Is ``0.05``.
   */
   function load_infer(url: string, model_id: string, ms1diff?: string, ms2diff?: string, intocutoff?: number): object;
   /**
   */
   function model_id(pool: object): string;
   /**
    * open the spectrum pool from a given resource link
    * 
    * 
     * @param link the resource string to the spectrum pool
     * @param model_id 
     * + default value Is ``null``.
     * @param score_overrides WARNING: this optional parameter will overrides the mode score 
     *  level when this parameter has a positive numeric value in 
     *  range ``(0,1]``. it is dangers to overrides the score parameter
     *  in the exists model.
     * 
     * + default value Is ``null``.
     * @param env 
     * + default value Is ``null``.
   */
   function openPool(link: string, model_id?: string, score_overrides?: number, env?: object): object;
   /**
     * @param prefix default value Is ``null``.
     * @param env default value Is ``null``.
   */
   function set_conservedGuid(spectral: any, prefix?: string, env?: object): any;
}
