﻿// export R# package module type define for javascript/typescript language
//
// ref=mzkit.FormulaTools@mzkit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null

/**
 * The chemical formulae toolkit
 * 
*/
declare namespace formula {
   /**
     * @param debug default value Is ``true``.
     * @param env default value Is ``null``.
   */
   function registerAnnotations(annotation:object, debug?:boolean, env?:object): any;
   /**
    * do peak annotation for the ms2 fragments
    * 
    * 
     * @param library A ms2 matrix object
     * @param massDiff -
     * 
     * + default value Is ``0.1``.
     * @param isotopeFirst -
     * 
     * + default value Is ``true``.
     * @param adducts -
     * 
     * + default value Is ``null``.
     * @param env 
     * + default value Is ``null``.
   */
   function peakAnnotations(library:any, massDiff?:number, isotopeFirst?:boolean, adducts?:object, env?:object): object;
   /**
    * find all of the candidate chemical formulas by a 
    *  specific exact mass value and a specific mass 
    *  tolerance value in ppm
    * 
    * 
     * @param mass the exact mass value
     * @param ppm the mass tolerance value in ppm
     * 
     * + default value Is ``5``.
     * @param candidateElements a list configuration of the formula candidates
     * 
     * + default value Is ``null``.
     * @param env 
     * + default value Is ``null``.
   */
   function candidates(mass:number, ppm?:number, candidateElements?:object, env?:object): object;
   /**
    * evaluate exact mass for the given formula strings.
    * 
    * 
     * @param formula a vector of the character formulas.
     * @param env 
     * + default value Is ``null``.
   */
   function eval(formula:any, env?:object): number;
   /**
    * Get atom composition from a formula string
    * 
    * 
     * @param formula The input formula string text.
     * @param env 
     * + default value Is ``null``.
   */
   function scan(formula:string, env?:object): object;
   /**
   */
   function canonical_formula(formula:object): string;
   /**
   */
   function getElementCount(formula:object, element:string): object;
   /**
   */
   function removeElement(formula:object, element:string): object;
   module read {
      /**
       * Read KCF model data
       * 
       * 
        * @param data The text data or file path
      */
      function KCF(data:string): object;
   }
   module KCF {
      /**
       * Create molecular network graph model
       * 
       * 
        * @param kcf The KCF molecule model
      */
      function graph(kcf:object): object;
   }
   module parse {
      /**
       * parse a single sdf text block
       * 
       * 
        * @param data -
        * @param parseStruct -
        * 
        * + default value Is ``true``.
      */
      function SDF(data:string, parseStruct?:boolean): object;
   }
   module SDF {
      /**
      */
      function convertKCF(sdfModel:object): object;
   }
   module open {
      module descriptor {
         /**
          * open the file handles of the chemical descriptor database.
          * 
          * 
           * @param dbFile A directory path which contains the multiple database file of the 
           *  chemical descriptors.
         */
         function db(dbFile:string): object;
      }
   }
   module descriptor {
      /**
        * @param env default value Is ``null``.
      */
      function matrix(repo:object, cid:object, env?:object): object;
   }
   /**
    * Parse the SMILES molecule structre string
    * 
    * > SMILES denotes a molecular structure as a graph with optional chiral 
    * >  indications. This is essentially the two-dimensional picture chemists
    * >  draw to describe a molecule. SMILES describing only the labeled
    * >  molecular graph (i.e. atoms and bonds, but no chiral or isotopic 
    * >  information) are known as generic SMILES.
    * 
     * @param SMILES -
     * @param strict -
     * 
     * + default value Is ``true``.
     * @return A chemical graph object that could be used for build formula or structure analysis
   */
   function parseSMILES(SMILES:string, strict?:boolean): object;
   module as {
      /**
      */
      function formula(SMILES:object): object;
   }
   /**
    * get atoms table from the SMILES structure data
    * 
    * 
     * @param SMILES -
   */
   function atoms(SMILES:object): object;
   /**
     * @param prob_threshold default value Is ``0.001``.
     * @param fwhm default value Is ``0.1``.
     * @param pad_left default value Is ``3``.
     * @param pad_right default value Is ``3``.
     * @param interpolate_grid default value Is ``0.005``.
   */
   function isotope_distribution(formula:any, prob_threshold?:number, fwhm?:number, pad_left?:number, pad_right?:number, interpolate_grid?:number): object;
}